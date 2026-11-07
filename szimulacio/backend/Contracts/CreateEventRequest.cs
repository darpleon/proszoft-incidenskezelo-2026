using System.Text.Json;
using backend.Models;

namespace backend.Contracts;

public sealed record CreateEventRequest
{
    public string ServiceCode { get; init; } = string.Empty;

    public string EventType { get; init; } = string.Empty;

    public string Severity { get; init; } = string.Empty;

    public DateTime? OccurredAtUtc { get; init; }

    public string? Summary { get; init; }

    public JsonElement? Payload { get; init; }

    public string? CorrelationKey { get; init; }

    public static CreateEventRequest FromSimulationEvent(SimulationEvent simulatedEvent)
    {
        return new CreateEventRequest
        {
            ServiceCode = simulatedEvent.ServiceCode,
            EventType = simulatedEvent.EventType.ToString(),
            Severity = simulatedEvent.Severity.ToString(),
            OccurredAtUtc = simulatedEvent.OccurredAtUtc,
            Summary = simulatedEvent.Summary,
            Payload = simulatedEvent.Payload is { } payload
                ? JsonSerializer.SerializeToElement(payload)
                : null,
            CorrelationKey = simulatedEvent.CorrelationKey,
        };
    }
}
