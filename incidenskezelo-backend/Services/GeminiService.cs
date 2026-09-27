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

        /*using var json = JsonDocument.Parse(responseBody);

        var outputText = json.RootElement
            .GetProperty("output_text")
            .GetString();

        return outputText ?? string.Empty; */

        return responseBody;
    }
}