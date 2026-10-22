using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Models.AI;
using Microsoft.Extensions.Options;

namespace incidenskezelo_backend.Services.AI;

public sealed class LocalAiService(
    HttpClient httpClient,
    IOptions<LocalAiOptions> options) : IAiService
{
    private readonly LocalAiOptions _options = options.Value;

    public async Task<AiTurnResponse> ExecuteTurnAsync(
        AiTurnRequest request,
        CancellationToken cancellationToken = default)
    {
        var messages = GetMessages(request);

        foreach (var toolResult in request.ToolResults)
        {
            messages.Add(new JsonObject
            {
                ["role"] = "tool",
                ["tool_name"] = toolResult.ToolName,
                ["content"] = toolResult.Result.GetRawText()
            });
        }

        var tools = new JsonArray();
        foreach (var tool in request.Tools)
        {
            tools.Add(new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject
                {
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["parameters"] = JsonNode.Parse(tool.ParametersSchemaJson)
                        ?? throw new InvalidOperationException(
                            $"Érvénytelen paraméterséma: {tool.Name}")
                }
            });
        }

        var body = new JsonObject
        {
            ["model"] = _options.Model,
            ["messages"] = messages,
            ["tools"] = tools,
            ["stream"] = false
        };

        var url = $"{_options.BaseUrl.TrimEnd('/')}/api/chat";
        using var response = await httpClient.PostAsJsonAsync(
            url,
            body,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Ollama API error ({(int)response.StatusCode}): {responseBody}");
        }

        using var json = JsonDocument.Parse(responseBody);
        var root = json.RootElement;

        if (!root.TryGetProperty("message", out var message))
        {
            throw new InvalidOperationException(
                "Az Ollama válaszából hiányzik a message mező.");
        }

        var assistantMessage = JsonNode.Parse(message.GetRawText())
            ?? throw new InvalidOperationException(
                "Az Ollama üres message objektumot adott vissza.");
        messages.Add(assistantMessage);

        var toolCalls = new List<AiToolCall>();
        if (message.TryGetProperty("tool_calls", out var calls))
        {
            foreach (var call in calls.EnumerateArray())
            {
                if (!call.TryGetProperty("function", out var function) ||
                    !function.TryGetProperty("name", out var nameElement))
                {
                    continue;
                }

                var arguments = function.TryGetProperty("arguments", out var argumentsElement)
                    ? argumentsElement.Clone()
                    : JsonSerializer.SerializeToElement(new { });

                toolCalls.Add(new AiToolCall(
                    Guid.NewGuid().ToString("N"),
                    nameElement.GetString() ?? string.Empty,
                    arguments));
            }
        }

        var text = message.TryGetProperty("content", out var contentElement)
            ? contentElement.GetString()
            : null;

        var state = JsonSerializer.SerializeToElement(new
        {
            messages
        });

        return new AiTurnResponse(
            string.IsNullOrWhiteSpace(text) ? null : text.Trim(),
            toolCalls,
            state);
    }

    private static JsonArray GetMessages(AiTurnRequest request)
    {
        if (request.ProviderState is { } state)
        {
            if (!state.TryGetProperty("messages", out var messagesElement) ||
                JsonNode.Parse(messagesElement.GetRawText()) is not JsonArray messages)
            {
                throw new InvalidOperationException(
                    "Az Ollama folytatásához hiányzik a messages előzmény.");
            }

            return messages;
        }

        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new InvalidOperationException(
                "Az első Ollama-körhöz prompt szükséges.");
        }

        return new JsonArray
        {
            new JsonObject
            {
                ["role"] = "user",
                ["content"] = request.Prompt
            }
        };
    }
}
