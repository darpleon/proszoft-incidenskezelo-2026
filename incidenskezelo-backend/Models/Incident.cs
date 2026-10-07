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

    public static Incident Create(string title, string description, IncidentPriority priority, ChangeContext context)
    {
        var incident = new Incident
        {
            Title = title,
            Description = description,
            Priority = priority,
            CreatedAtUtc = context.AtUtc,
            Status = IncidentStatus.Open
        };

        incident.StatusHistory.Add(new IncidentStatusChange(incident, null, IncidentStatus.Open, context.AtUtc));
        return incident;
    }

    public int IncidentId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IncidentStatus Status { get; private set; }

    public IncidentPriority Priority { get; set; } = IncidentPriority.Medium;

    public DateTime CreatedAtUtc { get; private set; }

    public List<IncidentStatusChange> StatusHistory { get; private set; } = [];

    public void TransitionTo(IncidentStatus next, ChangeContext context)
    {
        if (Status == IncidentStatus.Resolved || next != Status + 1)
        {
            throw new InvalidOperationException($"Invalid transition: {Status} -> {next}.");
        }

        StatusHistory.Add(new IncidentStatusChange(this, Status, next, context.AtUtc));
        Status = next;
    }
}
