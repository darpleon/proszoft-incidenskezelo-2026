using System.Net.Http.Json;
using System.Text.Json;
using incidenskezelo_backend.Configuration;
using Microsoft.Extensions.Options;

namespace incidenskezelo_backend.Services;

public class LocalAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly LocalAiOptions _options;

    public LocalAiService(
        HttpClient httpClient,
        IOptions<LocalAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_options.BaseUrl}/api/generate";

        var request = new
        {
            model = _options.Model,
            prompt,
            stream = false
        };

        using var response = await _httpClient.PostAsJsonAsync(
            url,
            request,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Ollama API error ({(int)response.StatusCode}): {responseBody}");
        }

        using var json = JsonDocument.Parse(responseBody);

        return json.RootElement
            .GetProperty("response")
            .GetString()
            ?? string.Empty;
    }
}