namespace backend.Models;

public class SimulationEvent
{
    public Guid EventId { get; set; }

    public string ServiceCode { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }

    public string? Summary { get; set; }

    public object? Payload { get; set; }
}