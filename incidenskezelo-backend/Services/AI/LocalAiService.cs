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


        if (string.IsNullOrWhiteSpace(_options.BaseUrl) ||
            string.IsNullOrWhiteSpace(_options.Model) ||
            !Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out _))
        {
            throw new AiProviderUnavailableException(
                "The local AI provider is not configured.");
        }

        try
        {
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

            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new AiInvalidResponseException(
                    "Ollama response must be a JSON object.");
            }

            if (!root.TryGetProperty("message", out var message) ||
                message.ValueKind != JsonValueKind.Object)
            {
                throw new AiInvalidResponseException(
                    "Ollama response is missing a valid message object.");
            }

            var assistantMessage = JsonNode.Parse(message.GetRawText())
                ?? throw new AiInvalidResponseException(
                    "Ollama returned an empty message object.");

            var toolCalls = new List<AiToolCall>();
            if (message.TryGetProperty("tool_calls", out var calls))
            {

                if (calls.ValueKind != JsonValueKind.Array)
                {
                    throw new AiInvalidResponseException(
                        "Ollama tool_calls must be an array.");
                }

                foreach (var call in calls.EnumerateArray())
                {
                    if (call.ValueKind != JsonValueKind.Object ||
                        !call.TryGetProperty("function", out var function) ||
                        function.ValueKind != JsonValueKind.Object ||
                        !function.TryGetProperty("name", out var nameElement) ||
                        nameElement.ValueKind != JsonValueKind.String ||
                        string.IsNullOrWhiteSpace(nameElement.GetString()))
                    {
                        throw new AiInvalidResponseException(
                            "Ollama returned an invalid tool call.");
                    }

                    var arguments = function.TryGetProperty(
                        "arguments",
                        out var argumentsElement)
                            ? argumentsElement.Clone()
                            : JsonSerializer.SerializeToElement(new { });

                    if (arguments.ValueKind != JsonValueKind.Object)
                    {
                        throw new AiInvalidResponseException(
                            "Ollama tool call arguments must be an object.");
                    }

                    toolCalls.Add(new AiToolCall(
                        Guid.NewGuid().ToString("N"),
                        nameElement.GetString() ?? string.Empty,
                        arguments));
                }
            }

            string? text = null;

            if (message.TryGetProperty("content", out var contentElement))
            {
                if (contentElement.ValueKind == JsonValueKind.String)
                {
                    text = contentElement.GetString();
                }
                else if (contentElement.ValueKind != JsonValueKind.Null)
                {
                    throw new AiInvalidResponseException(
                        "Ollama message content must be a string or null.");
                }
            }

            var state = JsonSerializer.SerializeToElement(new
            {
                messages
            });

            return new AiTurnResponse(
                string.IsNullOrWhiteSpace(text) ? null : text.Trim(),
                toolCalls,
                state);

        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new AiProviderTimeoutException(exception);
        }
        catch (HttpRequestException exception)
        {
            throw new AiProviderUnavailableException(
                "The local AI provider could not be reached.",
                exception);
        }
        catch (JsonException exception)
        {
            throw new AiInvalidResponseException(
                "Ollama returned invalid response data.",
                exception);
        }
    }

    private static JsonArray GetMessages(AiTurnRequest request)
    {
        if (request.ProviderState is { } state)
        {
            if (state.ValueKind != JsonValueKind.Object ||
                !state.TryGetProperty("messages", out var messagesElement))
            {
                throw new AiInvalidResponseException(
                    "Ollama continuation state is missing messages.");
            }

            JsonArray? messages;

            try
            {
                messages = JsonNode.Parse(messagesElement.GetRawText()) as JsonArray;
            }
            catch (JsonException exception)
            {
                throw new AiInvalidResponseException(
                    "Ollama continuation state contains invalid JSON.",
                    exception);
            }

            if (messages is null)
            {
                throw new AiInvalidResponseException(
                    "Ollama continuation messages must be an array.");
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
