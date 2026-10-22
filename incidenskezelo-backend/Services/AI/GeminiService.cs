using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Models.AI;
using Microsoft.Extensions.Options;

namespace incidenskezelo_backend.Services.AI;

public sealed class GeminiService(
    HttpClient httpClient,
    IOptions<GeminiOptions> options) : IAiService
{
    private const string InteractionsUrl =
        "https://generativelanguage.googleapis.com/v1beta/interactions";

    private readonly GeminiOptions _options = options.Value;

    public async Task<AiTurnResponse> ExecuteTurnAsync(
        AiTurnRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new JsonObject
        {
            ["model"] = _options.Model
        };

        if (request.ProviderState is { } providerState)
        {
            if (!providerState.TryGetProperty("interactionId", out var interactionIdElement) ||
                string.IsNullOrWhiteSpace(interactionIdElement.GetString()))
            {
                throw new InvalidOperationException(
                    "A Gemini folytatásához hiányzik az interactionId.");
            }

            body["previous_interaction_id"] = interactionIdElement.GetString();

            var input = new JsonArray();
            foreach (var toolResult in request.ToolResults)
            {
                input.Add(new JsonObject
                {
                    ["type"] = "function_result",
                    ["name"] = toolResult.ToolName,
                    ["call_id"] = toolResult.ToolCallId,
                    ["result"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["type"] = "text",
                            ["text"] = toolResult.Result.GetRawText()
                        }
                    }
                });
            }

            body["input"] = input;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                throw new InvalidOperationException(
                    "Az első Gemini-körhöz prompt szükséges.");
            }

            body["input"] = request.Prompt;
            body["generation_config"] = new JsonObject
            {
                ["tool_choice"] = "any"
            };
        }

        var tools = new JsonArray();
        foreach (var tool in request.Tools)
        {
            tools.Add(new JsonObject
            {
                ["type"] = "function",
                ["name"] = tool.Name,
                ["description"] = tool.Description,
                ["parameters"] = JsonNode.Parse(tool.ParametersSchemaJson)
                    ?? throw new InvalidOperationException(
                        $"Érvénytelen paraméterséma: {tool.Name}")
            });
        }

        body["tools"] = tools;

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            InteractionsUrl);
        httpRequest.Headers.Add("x-goog-api-key", _options.ApiKey);
        httpRequest.Content = new StringContent(
            body.ToJsonString(),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(
            httpRequest,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Gemini API error ({(int)response.StatusCode}): {responseBody}");
        }

        using var json = JsonDocument.Parse(responseBody);
        var root = json.RootElement;

        if (!root.TryGetProperty("id", out var responseIdElement) ||
            string.IsNullOrWhiteSpace(responseIdElement.GetString()))
        {
            throw new InvalidOperationException(
                "A Gemini válaszából hiányzik az interakció azonosítója.");
        }

        var toolCalls = new List<AiToolCall>();
        var output = new StringBuilder();

        if (root.TryGetProperty("steps", out var steps))
        {
            foreach (var step in steps.EnumerateArray())
            {
                if (!step.TryGetProperty("type", out var typeElement))
                {
                    continue;
                }

                var type = typeElement.GetString();
                if (type == "function_call" &&
                    step.TryGetProperty("id", out var callIdElement) &&
                    step.TryGetProperty("name", out var nameElement) &&
                    step.TryGetProperty("arguments", out var argumentsElement))
                {
                    toolCalls.Add(new AiToolCall(
                        callIdElement.GetString() ?? string.Empty,
                        nameElement.GetString() ?? string.Empty,
                        argumentsElement.Clone()));
                }
                else if (type == "model_output" &&
                         step.TryGetProperty("content", out var content))
                {
                    foreach (var item in content.EnumerateArray())
                    {
                        if (item.TryGetProperty("type", out var itemType) &&
                            itemType.GetString() == "text" &&
                            item.TryGetProperty("text", out var textElement))
                        {
                            output.AppendLine(textElement.GetString());
                        }
                    }
                }
            }
        }

        var state = JsonSerializer.SerializeToElement(new
        {
            interactionId = responseIdElement.GetString()
        });

        return new AiTurnResponse(
            string.IsNullOrWhiteSpace(output.ToString())
                ? null
                : output.ToString().Trim(),
            toolCalls,
            state);
    }
}
