namespace incidenskezelo_backend.Models;

public class RejectedEvent
{
    public long RejectedEventId { get; set; }

    public RejectedEventSource Source { get; set; }

    public RejectedEventReason Reason { get; set; }

    public string Error { get; set; } = string.Empty;

    public string RawPayload { get; set; } = string.Empty;

    public DateTime ReceivedAtUtc { get; set; }
}
