namespace backend.Models;

public class ScenarioStep
{
    // Ennyi másodpercet várjon ez előtt az esemény előtt.
    public int DelaySeconds { get; set; }

    public string CorrelationKey { get; set; } = string.Empty;

    public string ServiceCode { get; set; } = string.Empty;

    public EventType EventType { get; set; }

    public Severity Severity { get; set; }

    public string? Summary { get; set; }

    public object? Payload { get; set; }
}
