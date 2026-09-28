namespace backend.Models;

public class EventTemplate
{
    public string ServiceCode { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public object? Payload { get; set; }
}