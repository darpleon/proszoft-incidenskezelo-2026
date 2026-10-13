using System.Net.Http.Json;
using System.Text.Json;
using incidenskezelo_backend.Configuration;
using Microsoft.Extensions.Options;

namespace incidenskezelo_backend.Services;

public class GeminiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    public GeminiService(
        HttpClient httpClient,
        IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        const string url =
            "https://generativelanguage.googleapis.com/v1beta/interactions";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        request.Headers.Add(
            "x-goog-api-key",
            _options.ApiKey);

        request.Content = JsonContent.Create(new
        {
            model = _options.Model,
            input = prompt
        });

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Gemini API error ({(int)response.StatusCode}): {responseBody}");
        }

        using var json = JsonDocument.Parse(responseBody);

        if (!json.RootElement.TryGetProperty("steps", out var steps))
        {
            throw new InvalidOperationException(
                "A Gemini válaszából hiányzik a steps mező.");
        }

        var output = new System.Text.StringBuilder();

        foreach (var step in steps.EnumerateArray())
        {
            if (!step.TryGetProperty("type", out var typeElement) ||
                typeElement.GetString() != "model_output")
            {
                continue;
            }

            if (!step.TryGetProperty("content", out var content))
            {
                continue;
            }

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

        var result = output.ToString().Trim();

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException(
                "A Gemini nem adott vissza szöveges választ.");
        }

        return result;


        
    }
}