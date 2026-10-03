namespace incidenskezelo_backend.Models;

public class Event
{
    public long EventId { get; set; }

    public string SourceEventId { get; set; } = string.Empty;

    public int ServiceId { get; set; }

    public Service Service { get; set; } = null!;

    public EventType EventType { get; set; }

    public Severity Severity { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public DateTime ReceivedAtUtc { get; set; }

    public string? Summary { get; set; }

    public string? PayloadJson { get; set; }
}
