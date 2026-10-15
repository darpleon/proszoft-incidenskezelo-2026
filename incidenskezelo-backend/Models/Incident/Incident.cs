using System.Collections.Frozen;
using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Models.Incident;

public class Incident
{
    private static readonly FrozenDictionary<IncidentStatus, IncidentStatus> NextStatus =
        new Dictionary<IncidentStatus, IncidentStatus>
        {
            [IncidentStatus.Open] = IncidentStatus.Investigating,
            [IncidentStatus.Investigating] = IncidentStatus.InProgress,
            [IncidentStatus.InProgress] = IncidentStatus.Verifying,
            [IncidentStatus.Verifying] = IncidentStatus.Resolved
        }.ToFrozenDictionary();

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
        if (!NextStatus.TryGetValue(Status, out var allowed) || next != allowed)
        {
            throw new InvalidOperationException($"Invalid transition: {Status} -> {next}.");
        }

        StatusHistory.Add(new IncidentStatusChange(this, Status, next, context.AtUtc));
        Status = next;
    }
}
