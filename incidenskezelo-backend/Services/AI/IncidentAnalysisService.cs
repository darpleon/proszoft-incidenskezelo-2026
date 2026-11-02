using System.Text.Json;
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Models.AI;
using incidenskezelo_backend.Services;
using incidenskezelo_backend.Contracts;
using System.Text.Json.Serialization;
using System.Linq;

namespace incidenskezelo_backend.Services.AI;

public sealed class IncidentAnalysisService(
    AiServiceResolver aiServiceResolver,
    IIncidentService incidentService,
    EventService eventService) : IIncidentAnalysisService
{
    private const int MaxToolRounds = 5;
    private const string GetIncidentDetailsName = "getIncidentDetails";
    private const string GetRelatedEventsName = "getRelatedEvents";

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
            """)
    ];

    public async Task<IncidentAnalysisResult> AnalyzeAsync(
        Guid incidentId,
        AiProvider provider,
        CancellationToken cancellationToken = default)
    {
        var toolCallStatuses = new List<AiToolCallStatus>();
        var relevantEventIds = new HashSet<long>();
        var aiService = aiServiceResolver.Resolve(provider);
        var prompt = $"""
            {IncidentAnalysisPrompt.SystemPrompt}

            Incident ID to analyze:
            {incidentId}
            """;

        var request = new AiTurnRequest(
            prompt,
            ProviderState: null,
            Tools,
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
                    throw new InvalidOperationException(
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
                    throw new InvalidOperationException(
                        "The AI returned invalid analysis JSON.",
                        exception);
                }

                if (payload is null || string.IsNullOrWhiteSpace(payload.Summary))
                {
                    throw new InvalidOperationException(
                        "The AI returned invalid analysis JSON: summary is required.");
                }

                var probableCauses = payload.ProbableCauses ?? [];

                foreach (var cause in probableCauses)
                {
                    if (string.IsNullOrWhiteSpace(cause.Description))
                    {
                        throw new InvalidOperationException(
                            "The AI returned invalid analysis JSON: cause description is required.");
                    }

                    if (cause.Confidence is not ("High" or "Medium" or "Low"))
                    {
                        throw new InvalidOperationException(
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

                return new IncidentAnalysisResult(
                    Model: model,
                    Summary: payload.Summary,
                    ProbableCauses: probableCauses
                        .Select(cause => new ProbableCause(
                            cause.Description!,
                            cause.Confidence!))
                        .ToList(),
                    SuggestedActions: payload.SuggestedActions ?? [],
                    RelevantEventIds: verifiedEventIds,
                    Limitations: payload.Limitations ?? [],
                    ToolCalls: toolCallStatuses);
            }

            if (response.ProviderState is null)
            {
                throw new InvalidOperationException(
                    "The AI tool call response is missing the continuation state.");
            }

            var results = new List<AiToolResult>();
            foreach (var toolCall in response.ToolCalls)
            {
                try
                {
                    JsonElement result;

                    if (toolCall.Name == GetIncidentDetailsName)
                    {
                        var incident = await incidentService.GetByIdAsync(
                            incidentId,
                            cancellationToken);

                        result = JsonSerializer.SerializeToElement(
                            new
                            {
                                found = incident is not null,
                                incident
                            },
                            ToolResultJsonOptions);
                    }
                    else if (toolCall.Name == GetRelatedEventsName)
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
                Tools,
                results);
        }

        throw new InvalidOperationException(
            $"The AI exceeded the maximum of {MaxToolRounds} tool-call rounds.");
    }

    private sealed record AiAnalysisPayload(
    string? Summary,
    List<AiProbableCausePayload>? ProbableCauses,
    List<string>? SuggestedActions,
    List<long>? RelevantEventIds,
    List<string>? Limitations);

    private sealed record AiProbableCausePayload(
        string? Description,
        string? Confidence);
}
