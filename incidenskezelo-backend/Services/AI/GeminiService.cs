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

        if (string.IsNullOrWhiteSpace(_options.ApiKey) ||
            string.IsNullOrWhiteSpace(_options.Model))
        {
            throw new AiProviderUnavailableException(
                "Gemini is not configured.");
        }

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
                    "The interactionId is required to continue the Gemini interaction.");
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
                    "A prompt is required for the first Gemini turn.");
            }

            body["input"] = request.Prompt;
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
                        $"Invalid parameter schema: {tool.Name}")
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

        try

        {
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

            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new AiInvalidResponseException(
                    "Gemini response must be a JSON object.");
            }

            if (!root.TryGetProperty("id", out var responseIdElement) ||
                responseIdElement.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(responseIdElement.GetString()))
            {
                throw new AiInvalidResponseException(
                    "The interaction ID is missing or invalid.");
            }

            var toolCalls = new List<AiToolCall>();
            var output = new StringBuilder();

            if (root.TryGetProperty("steps", out var steps))
            {

                if (steps.ValueKind != JsonValueKind.Array)
                {
                    throw new AiInvalidResponseException(
                        "Gemini response steps must be an array.");
                }

                foreach (var step in steps.EnumerateArray())
                {
                    if (step.ValueKind != JsonValueKind.Object ||
                        !step.TryGetProperty("type", out var typeElement) ||
                        typeElement.ValueKind != JsonValueKind.String)
                    {
                        throw new AiInvalidResponseException(
                            "Gemini returned an invalid response step.");
                    }

                    var type = typeElement.GetString();

                    if (type == "function_call")
                    {
                        if (!step.TryGetProperty("id", out var callIdElement) ||
                            callIdElement.ValueKind != JsonValueKind.String ||
                            !step.TryGetProperty("name", out var nameElement) ||
                            nameElement.ValueKind != JsonValueKind.String ||
                            !step.TryGetProperty("arguments", out var argumentsElement) ||
                            argumentsElement.ValueKind != JsonValueKind.Object)
                        {
                            throw new AiInvalidResponseException(
                                "Gemini returned an invalid function call.");
                        }

                        toolCalls.Add(new AiToolCall(
                            callIdElement.GetString()!,
                            nameElement.GetString()!,
                            argumentsElement.Clone()));
                    }
                    else if (type == "model_output")
                    {
                        if (!step.TryGetProperty("content", out var content) ||
                            content.ValueKind != JsonValueKind.Array)
                        {
                            throw new AiInvalidResponseException(
                                "Gemini returned invalid model output content.");
                        }

                        foreach (var item in content.EnumerateArray())
                        {
                            if (item.ValueKind != JsonValueKind.Object ||
                                !item.TryGetProperty("type", out var itemType) ||
                                itemType.ValueKind != JsonValueKind.String)
                            {
                                throw new AiInvalidResponseException(
                                    "Gemini returned an invalid content item.");
                            }

                            if (itemType.GetString() == "text")
                            {
                                if (!item.TryGetProperty("text", out var textElement) ||
                                    textElement.ValueKind != JsonValueKind.String)
                                {
                                    throw new AiInvalidResponseException(
                                        "Gemini returned an invalid text item.");
                                }

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
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new AiProviderTimeoutException(exception);
        }
        catch (HttpRequestException exception)
        {
            throw new AiProviderUnavailableException(
                "Gemini could not be reached.",
                exception);
        }
        catch (JsonException exception)
        {
            throw new AiInvalidResponseException(
                "Gemini returned invalid response data.",
                exception);
        }
    }
}
