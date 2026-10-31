using System.Text.Json;
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Models.AI;
using incidenskezelo_backend.Services;

namespace incidenskezelo_backend.Services.AI;

public sealed class IncidentAnalysisService(
    AiServiceResolver aiServiceResolver,
    IIncidentService incidentService) : IIncidentAnalysisService
{
    private const int MaxToolRounds = 5;
    private const string GetIncidentDetailsName = "getIncidentDetails";

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
            """)
    ];

    public async Task<string> AnalyzeAsync(
        Guid incidentId,
        AiProvider provider,
        CancellationToken cancellationToken = default)
    {
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

                return response.Text;
            }

            if (response.ProviderState is null)
            {
                throw new InvalidOperationException(
                    "The AI tool call response is missing the continuation state.");
            }

            var results = new List<AiToolResult>();
            foreach (var toolCall in response.ToolCalls)
            {
                if (toolCall.Name != GetIncidentDetailsName)
                {
                    throw new NotSupportedException(
                        $"Unsupported AI tool: {toolCall.Name}");
                }

                var incident = await incidentService.GetByIdAsync(
                    incidentId,
                    cancellationToken);

                var result = JsonSerializer.SerializeToElement(new
                {
                    found = incident is not null,
                    incident
                });

                results.Add(new AiToolResult(
                    toolCall.Id,
                    toolCall.Name,
                    result));
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
}
