using backend.Models;

namespace backend.Services;

public class SimulatorService
{
    private int _eventsGenerated;

    private readonly List<EventTemplate> _eventTemplates =
    [
        new EventTemplate
        {
            ServiceCode = "payment-gw",
            EventType = EventType.HighLatency,
            Severity = Severity.Error,
            Summary = "Payment service timeout"
        },

        new EventTemplate
        {
            ServiceCode = "payment-gw",
            EventType = EventType.ServiceDown,
            Severity = Severity.Critical,
            Summary = "Payment service connection error"
        },

        new EventTemplate
        {
            ServiceCode = "payment-gw",
            EventType = EventType.ServiceRecovered,
            Severity = Severity.Info,
            Summary = "Payment gateway is available again"
        },

        new EventTemplate
        {
            ServiceCode = "web-portal",
            EventType = EventType.HighErrorRate,
            Severity = Severity.Warning,
            Summary = "Multiple login failures detected"
        },

        new EventTemplate
        {
            ServiceCode = "postgres-main",
            EventType = EventType.DbConnectionError,
            Severity = Severity.Critical,
            Summary = "Connection pool exhausted",
            Payload = new { poolSize = 20, waitingRequests = 47 }
        },

        new EventTemplate
        {
            ServiceCode = "orders-api",
            EventType = EventType.HighLatency,
            Severity = Severity.Warning,
            Summary = "Response time above threshold",
            Payload = new { p95Ms = 1800 }
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
