using System.Text.Json;
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Models.AI;
using incidenskezelo_backend.Services;

namespace incidenskezelo_backend.Services.AI;

public sealed class IncidentAnalysisService(
    IAiService aiService,
    IIncidentService incidentService) : IIncidentAnalysisService
{
    private const int MaxToolRounds = 5;
    private const string GetIncidentDetailsName = "getIncidentDetails";

    private static readonly IReadOnlyList<AiToolDefinition> Tools =
    [
        new AiToolDefinition(
            GetIncidentDetailsName,
            "Lekéri az éppen elemzett incidens adatait.",
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
        CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            {IncidentAnalysisPrompt.SystemPrompt}

            Az elemzendő incidens azonosítója:
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
                        "Az AI nem adott vissza szöveges elemzést.");
                }

                return response.Text;
            }

            if (response.ProviderState is null)
            {
                throw new InvalidOperationException(
                    "Az AI eszközhívási válaszából hiányzik a folytatási állapot.");
            }

            var results = new List<AiToolResult>();
            foreach (var toolCall in response.ToolCalls)
            {
                if (toolCall.Name != GetIncidentDetailsName)
                {
                    throw new NotSupportedException(
                        $"Nem támogatott AI-eszköz: {toolCall.Name}");
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
            $"Az AI túllépte a megengedett {MaxToolRounds} eszközhívási kört.");
    }
}
