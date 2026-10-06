namespace incidenskezelo_backend.Models;

public enum IncidentStatus
{
    Open,
    Investigating,
    InProgress,
    Verifying,
    Resolved
}

public enum IncidentPriority
{
    Low,
    Medium,
    High,
    Critical
}

public class Incident
{
    private Incident() { }

    public static Incident Create(string title, string description, IncidentPriority priority)
    {
        return new Incident
        {
            Title = title,
            Description = description,
            Priority = priority,
            Status = IncidentStatus.Open
        };
    }

    public int IncidentId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IncidentStatus Status { get; private set; }

    public IncidentPriority Priority { get; set; } = IncidentPriority.Medium;

    public void TransitionTo(IncidentStatus next)
    {
        if (Status == IncidentStatus.Resolved || next != Status + 1)
        {
            throw new InvalidOperationException($"Invalid transition: {Status} -> {next}.");
        }

        Status = next;
    }
}
