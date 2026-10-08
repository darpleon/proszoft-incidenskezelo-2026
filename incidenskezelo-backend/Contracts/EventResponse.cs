using incidenskezelo_backend.Models;

namespace incidenskezelo_backend.Contracts;

public class EventResponse
{
    public long EventId { get; set; }

    public string SourceEventId { get; set; } = string.Empty;

    public string ServiceCode { get; set; } = string.Empty;

    public EventType EventType { get; set; }

    public Severity Severity { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public DateTime ReceivedAtUtc { get; set; }

    public string? Summary { get; set; }

    public string? PayloadJson { get; set; }
}
