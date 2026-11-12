using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models.AI;
using incidenskezelo_backend.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace incidenskezelo_backend.Services.AI;

public sealed class IncidentAnalysisService(
    AiServiceResolver aiServiceResolver,
    IIncidentService incidentService,
    EventService eventService,
    AppDbContext dbContext,
    TimeProvider timeProvider) : IIncidentAnalysisService
{
    private const int MaxToolRounds = 5;
    private const int MaxToolCalls = 5;
    private const string GetIncidentDetailsName = "getIncidentDetails";
    private const string GetRelatedEventsName = "getRelatedEvents";
    private const string GetIncidentHistoryName = "getIncidentHistory";
    private const string GetRecentServiceEventsName = "getRecentServiceEvents";
    private const string SearchPreviousIncidentsName = "searchPreviousIncidents";
    private const int MaxPreviousIncidentSearchLimit = 5;
    private const int MaxPreviousIncidentQueryLength = 200;

    private static readonly JsonSerializerOptions ToolResultJsonOptions =
    new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly IReadOnlyList<AiToolDefinition> Tools =
    [
        new AiToolDefinition(
            GetIncidentDetailsName,
            "Retrieves the details of the incident being analyzed.",
            """
            {
              "type": "object",
              "properties": {},
              "additionalProperties": false
            }
            """),

        new AiToolDefinition(
            GetRelatedEventsName,
            "Retrieves the events linked to the incident being analyzed.",
            """
            {
              "type": "object",
              "properties": {},
              "additionalProperties": false
            }
            """),

        new AiToolDefinition(
            GetIncidentHistoryName,
            "Retrieves the chronological change history of the incident being analyzed, including its creation and changes to status, title, description, priority, and assignee.",
            """
            {
              "type": "object",
              "properties": {},
              "additionalProperties": false
            }
            """),

        new AiToolDefinition(
            GetRecentServiceEventsName,
            "Retrieves the most recent events for the specified service, whether or not they are linked to the incident being analyzed.",
            """
            {
              "type": "object",
              "properties": {
                "serviceCode": {
                  "type": "string",
                  "description": "The service code, such as orders-api or postgres-main."
                },
                "limit": {
                  "type": "integer",
                  "description": "Maximum number of recent events to return, from 1 to 20.",
                  "minimum": 1,
                  "maximum": 20
                }
              },
              "required": ["serviceCode", "limit"],
              "additionalProperties": false
            }
            """),
        new AiToolDefinition(
            SearchPreviousIncidentsName,
            "Searches other incidents by phrase in their title or description. The incident currently being analyzed is excluded.",
            """
            {
              "type": "object",
              "properties": {
                "query": {
                  "type": "string",
                  "description": "A concise distinctive phrase from the incident title, description, or related event summaries."
                },
                "limit": {
                  "type": "integer",
                  "description": "Maximum number of matching incidents to return, from 1 to 5.",
                  "minimum": 1,
                  "maximum": 5
                }
              },
              "required": ["query", "limit"],
              "additionalProperties": false
            }
            """)
    ];

    public async Task<IncidentAnalysisResult> AnalyzeAsync(
        Guid incidentId,
        AiProvider provider,
        CancellationToken cancellationToken = default)
    {
        var executedToolCallCount = 0;
        var toolCallStatuses = new List<AiToolCallStatus>();
        var relevantEventIds = new HashSet<long>();
        var aiService = aiServiceResolver.Resolve(provider);

        // A backend kötelezően lekéri az incidens adatait.
        var incident = await incidentService.GetByIdAsync(
            incidentId,
            cancellationToken);

        if (incident is null)
        {
            var model = provider switch
            {
                AiProvider.Gemini => "Gemini",
                AiProvider.Qwen3 => "Qwen3",
                _ => throw new ArgumentOutOfRangeException(nameof(provider))
            };

            return new IncidentAnalysisResult(
                Model: model,
                Summary: $"Incident {incidentId} could not be found.",
                ProbableCauses: [],
                SuggestedActions: [],
                RelevantEventIds: [],
                Limitations:
                [
                    "The incident could not be found, so no analysis was performed."
                ],
                ToolCalls:
                [
                    new AiToolCallStatus(
                        GetIncidentDetailsName,
                        Succeeded: true)
                ]);
        }

        var incidentDetails = JsonSerializer.SerializeToElement(
            new
            {
                found = true,
                incident
            },
            ToolResultJsonOptions);

        // A toolCalls a backend által ténylegesen elvégzett lekérést jelzi.
        toolCallStatuses.Add(new AiToolCallStatus(
            GetIncidentDetailsName,
            Succeeded: true));

        executedToolCallCount = 1;

        var availableTools = Tools
            .Where(tool => tool.Name != GetIncidentDetailsName)
            .ToArray();

        var prompt = $"""
            {IncidentAnalysisPrompt.SystemPrompt}

            The backend has already executed getIncidentDetails.
            Use this result as the source of truth for the incident.
            Do not call getIncidentDetails again.
            You may make no more than 4 additional tool calls.
            Do not infer or invent incident details.

            getIncidentDetails result:
            {incidentDetails.GetRawText()}
            """;

        var request = new AiTurnRequest(
            prompt,
            ProviderState: null,
            availableTools,
            ToolResults: []);


        for (var round = 0; round < MaxToolRounds; round++)
        {
            var response = await aiService.ExecuteTurnAsync(
                request,
                cancellationToken);

            if (response.ToolCalls.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(response.Text))
                {
                    throw new AiInvalidResponseException(
                        "The AI did not return a text analysis.");
                }

                AiAnalysisPayload? payload;

                try
                {
                    payload = JsonSerializer.Deserialize<AiAnalysisPayload>(
                        response.Text,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web));
                }
                catch (JsonException exception)
                {
                    throw new AiInvalidResponseException(
                        "The AI returned invalid analysis JSON.",
                        exception);
                }

                if (payload is null || string.IsNullOrWhiteSpace(payload.Summary))
                {
                    throw new AiInvalidResponseException(
                        "The AI returned invalid analysis JSON: summary is required.");
                }

                var probableCauses = payload.ProbableCauses ?? [];

                foreach (var cause in probableCauses)
                {
                    if (cause is null ||
                        string.IsNullOrWhiteSpace(cause.Description))
                    {
                        throw new AiInvalidResponseException(
                            "The AI returned invalid analysis JSON: cause description is required.");
                    }

                    if (cause.Confidence is not ("High" or "Medium" or "Low"))
                    {
                        throw new AiInvalidResponseException(
                            "The AI returned invalid analysis JSON: confidence must be High, Medium, or Low.");
                    }
                }

                var verifiedEventIds = (payload.RelevantEventIds ?? [])
                    .Where(relevantEventIds.Contains)
                    .Distinct()
                    .ToList();

                var model = provider switch
                {
                    AiProvider.Gemini => "Gemini",
                    AiProvider.Qwen3 => "Qwen3",
                    _ => throw new ArgumentOutOfRangeException(nameof(provider))
                };

                var result = new IncidentAnalysisResult(
                    Model: model,
                    Summary: payload.Summary,
                    ProbableCauses: probableCauses
                        .Select(cause => new ProbableCause(
                            cause!.Description!,
                            cause.Confidence!))
                        .ToList(),
                    SuggestedActions: payload.SuggestedActions ?? [],
                    RelevantEventIds: verifiedEventIds,
                    Limitations: payload.Limitations ?? [],
                    ToolCalls: toolCallStatuses);

                await SaveLatestAsync(incidentId, result, cancellationToken);

                return result;
            }

            if (response.ProviderState is null)
            {
                throw new AiInvalidResponseException(
                    "The AI tool call response is missing the continuation state.");
            }

            var results = new List<AiToolResult>();
            foreach (var toolCall in response.ToolCalls)
            {
                if (executedToolCallCount >= MaxToolCalls)
                {
                    results.Add(new AiToolResult(
                        toolCall.Id,
                        toolCall.Name,
                        JsonSerializer.SerializeToElement(
                            new
                            {
                                succeeded = false,
                                error = "Maximum tool call limit reached."
                            },
                            ToolResultJsonOptions)));

                    toolCallStatuses.Add(new AiToolCallStatus(
                        toolCall.Name,
                        Succeeded: false));

                    continue;
                }

                executedToolCallCount++;
                try
                {
                    JsonElement result;

                    if (toolCall.Name == GetRelatedEventsName)
                    {
                        var events = await eventService.GetByIncidentAsync(
                            incidentId,
                            cancellationToken);

                        if (events is not null)
                        {
                            foreach (var incidentEvent in events)
                            {
                                relevantEventIds.Add(incidentEvent.EventId);
                            }
                        }

                        result = JsonSerializer.SerializeToElement(
                            new
                            {
                                incidentFound = events is not null,
                                events = events ?? new List<EventResponse>()
                            },
                            ToolResultJsonOptions);
                    }
                    else if (toolCall.Name == GetIncidentHistoryName)
                    {
                        var history = await incidentService.GetHistoryAsync(
                            incidentId,
                            cancellationToken);

                        result = JsonSerializer.SerializeToElement(
                            new
                            {
                                found = history is not null,
                                history = history ?? []
                            },
                            ToolResultJsonOptions);
                    }
                    else if (toolCall.Name == GetRecentServiceEventsName)
                    {
                        var arguments = toolCall.Arguments;

                        if (!arguments.TryGetProperty("serviceCode", out var serviceCodeElement) ||
                            serviceCodeElement.ValueKind != JsonValueKind.String)
                        {
                            throw new ArgumentException(
                                "The serviceCode argument is required.");
                        }

                        var serviceCode = serviceCodeElement.GetString()?.Trim();

                        if (string.IsNullOrWhiteSpace(serviceCode))
                        {
                            throw new ArgumentException(
                                "The serviceCode argument cannot be empty.");
                        }

                        if (serviceCode.Length > 50)
                        {
                            throw new ArgumentException(
                                "The serviceCode argument cannot exceed 50 characters.");
                        }

                        if (!arguments.TryGetProperty("limit", out var limitElement) ||
                            !limitElement.TryGetInt32(out var limit) ||
                            limit is < 1 or > 20)
                        {
                            throw new ArgumentException(
                                "The limit argument must be an integer between 1 and 20.");
                        }

                        var events = await eventService.GetRecentByServiceCodeAsync(
                            serviceCode,
                            limit,
                            cancellationToken);

                        foreach (var incidentEvent in events)
                        {
                            relevantEventIds.Add(incidentEvent.EventId);
                        }

                        result = JsonSerializer.SerializeToElement(
                            new
                            {
                                serviceCode,
                                events
                            },
                            ToolResultJsonOptions);
                    }
                    else if (toolCall.Name == SearchPreviousIncidentsName)
                    {
                        var arguments = toolCall.Arguments;

                        if (!arguments.TryGetProperty("query", out var queryElement) ||
                            queryElement.ValueKind != JsonValueKind.String)
                        {
                            throw new ArgumentException(
                                "The query argument is required.");
                        }

                        var query = queryElement.GetString()?.Trim();

                        if (string.IsNullOrWhiteSpace(query))
                        {
                            throw new ArgumentException(
                                "The query argument cannot be empty.");
                        }

                        if (query.Length > MaxPreviousIncidentQueryLength)
                        {
                            throw new ArgumentException(
                                $"The query cannot exceed {MaxPreviousIncidentQueryLength} characters.");
                        }

                        if (!arguments.TryGetProperty("limit", out var limitElement) ||
                            !limitElement.TryGetInt32(out var limit) ||
                            limit is < 1 or > MaxPreviousIncidentSearchLimit)
                        {
                            throw new ArgumentException(
                                $"The limit must be an integer between 1 and {MaxPreviousIncidentSearchLimit}.");
                        }

                        var incidents = await incidentService.SearchPreviousAsync(
                            incidentId,
                            query,
                            limit,
                            cancellationToken);

                        result = JsonSerializer.SerializeToElement(
                            new
                            {
                                query,
                                incidents
                            },
                            ToolResultJsonOptions);
                    }
                    else
                    {
                        throw new NotSupportedException(
                            $"Unsupported AI tool: {toolCall.Name}");
                    }

                    results.Add(new AiToolResult(
                        toolCall.Id,
                        toolCall.Name,
                        result));

                    toolCallStatuses.Add(new AiToolCallStatus(
                        toolCall.Name,
                        Succeeded: true));
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    toolCallStatuses.Add(new AiToolCallStatus(
                        toolCall.Name,
                        Succeeded: false));

                    results.Add(new AiToolResult(
                        toolCall.Id,
                        toolCall.Name,
                        JsonSerializer.SerializeToElement(
                            new
                            {
                                succeeded = false,
                                error = "Tool execution failed."
                            },
                            ToolResultJsonOptions)));
                }
            }

            request = new AiTurnRequest(
                Prompt: null,
                response.ProviderState,
                availableTools,
                results);
        }

        throw new AiInvalidResponseException(
            $"The AI exceeded the maximum of {MaxToolRounds} tool-call rounds.");
    }

    private sealed record AiAnalysisPayload(
    string? Summary,
    List<AiProbableCausePayload?>? ProbableCauses,
    List<string>? SuggestedActions,
    List<long>? RelevantEventIds,
    List<string>? Limitations);

    private sealed record AiProbableCausePayload(
        string? Description,
        string? Confidence);

    private async Task SaveLatestAsync(
    Guid publicIncidentId,
    IncidentAnalysisResult result,
    CancellationToken cancellationToken)
    {
        var internalIncidentId = await dbContext.Incidents
            .Where(incident => incident.PublicId == publicIncidentId)
            .Select(incident => (int?)incident.IncidentId)
            .SingleOrDefaultAsync(cancellationToken);

        // Ha az incidens nem létezik, ne próbáljunk idegen kulcsot menteni.
        // A jelenlegi elemzési válasz továbbra is visszaadható.
        if (internalIncidentId is null)
        {
            return;
        }

        var stored = await dbContext.StoredIncidentAnalyses
            .SingleOrDefaultAsync(
                analysis => analysis.IncidentId == internalIncidentId.Value,
                cancellationToken);

        var resultJson = JsonSerializer.Serialize(
            result,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var isInsert = stored is null;

        if (stored is null)
        {
            stored = new StoredIncidentAnalysis
            {
                IncidentId = internalIncidentId.Value,
                ResultJson = resultJson,
                AnalyzedAtUtc = timeProvider.GetUtcNow().UtcDateTime
            };

            dbContext.StoredIncidentAnalyses.Add(stored);
        }
        else
        {
            stored.ResultJson = resultJson;
            stored.AnalyzedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (isInsert &&
                  ex.InnerException is SqlException { Number: 2601 or 2627 } &&
                  ex.Entries.Count == 1 &&
                  ReferenceEquals(ex.Entries[0].Entity, stored))
        {
            dbContext.Entry(stored).State = EntityState.Detached;
        }
    }

    public async Task<StoredIncidentAnalysis?> GetLatestAsync(
    Guid publicIncidentId,
    CancellationToken cancellationToken)
    {
        var internalIncidentId = await dbContext.Incidents
            .Where(incident => incident.PublicId == publicIncidentId)
            .Select(incident => (int?)incident.IncidentId)
            .SingleOrDefaultAsync(cancellationToken);

        if (internalIncidentId is null)
        {
            return null;
        }

        return await dbContext.StoredIncidentAnalyses
            .AsNoTracking()
            .SingleOrDefaultAsync(
                analysis => analysis.IncidentId == internalIncidentId.Value,
                cancellationToken);
    }
}
