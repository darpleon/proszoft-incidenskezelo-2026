namespace backend.Models;

public class SimulationEvent
{
    public string ServiceCode { get; set; } = string.Empty;

    public EventType EventType { get; set; }

    public Severity Severity { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public string? Summary { get; set; }

    public object? Payload { get; set; }

    public string CorrelationKey { get; set; } = string.Empty;
}