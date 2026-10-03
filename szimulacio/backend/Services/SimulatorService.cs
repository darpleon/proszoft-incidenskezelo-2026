using backend.Models;

namespace backend.Services;

public class SimulatorService
{
    private int _eventsGenerated;

    private readonly List<EventTemplate> _eventTemplates =
    [
        new EventTemplate
        {
            ServiceCode = "PAYMENT",
            EventType = "Timeout",
            Severity = "Critical",
            Summary = "Payment service timeout"
        },

        new EventTemplate
        {
            ServiceCode = "PAYMENT",
            EventType = "ConnectionError",
            Severity = "Error",
            Summary = "Payment service connection error"
        },

        new EventTemplate
        {
            ServiceCode = "AUTH",
            EventType = "LoginFailure",
            Severity = "Warning",
            Summary = "Multiple login failures detected"
        }
    ];

    public SimulationEvent GenerateEvent()
    {
        // Véletlenszerűen kiválasztunk egy sablont
        var template = _eventTemplates[
            Random.Shared.Next(_eventTemplates.Count)
        ];

        // A kiválasztott sablonból létrehozunk egy új eseményt
        var simulatedEvent = new SimulationEvent
        {
            EventId = Guid.NewGuid(),

            ServiceCode = template.ServiceCode,

            EventType = template.EventType,

            Severity = template.Severity,

            OccurredAtUtc = DateTime.UtcNow,

            Summary = template.Summary,

            Payload = template.Payload
        };

        _eventsGenerated++;

        return simulatedEvent;
    }

    public SimulatorStatus GetStatus()
    {
        return new SimulatorStatus
        {
            InstanceId = "local-simulator",
            Running = true,
            EventsGenerated = _eventsGenerated,
            IntervalSeconds = 5
        };
    }
}
