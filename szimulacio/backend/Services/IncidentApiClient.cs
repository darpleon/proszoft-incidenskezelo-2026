using backend.Configuration;
using backend.Models;
using Microsoft.Extensions.Options;

namespace backend.Services;

public class IncidentApiClient
{
    private readonly HttpClient _httpClient;

    public IncidentApiClient(HttpClient httpClient, IOptions<IncidentApiOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
    }


    public async Task<HttpResponseMessage> SendEventAsync(SimulationEvent simulatedEvent, CancellationToken cancellationToken)
    {
        var body = new
        {
            sourceEventId = simulatedEvent.EventId.ToString(),
            serviceCode = simulatedEvent.ServiceCode,
            eventType = simulatedEvent.EventType.ToString(),
            severity = simulatedEvent.Severity.ToString(),
            occurredAtUtc = simulatedEvent.OccurredAtUtc,
            summary = simulatedEvent.Summary,
            payload = simulatedEvent.Payload,
        };

        return await _httpClient.PostAsJsonAsync("api/events", body, cancellationToken);
    }
}
