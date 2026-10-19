using System.Collections.Frozen;
using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Models.Incident;

public class Incident
{
    private static readonly FrozenDictionary<IncidentStatus, FrozenSet<IncidentStatus>> AllowedTransitions =
        new Dictionary<IncidentStatus, FrozenSet<IncidentStatus>>
        {
            [IncidentStatus.Open] = new[] { IncidentStatus.Investigating, IncidentStatus.Declined }.ToFrozenSet(),
            [IncidentStatus.Investigating] = new[] { IncidentStatus.InProgress, IncidentStatus.Declined }.ToFrozenSet(),
            [IncidentStatus.InProgress] = new[] { IncidentStatus.Verifying, IncidentStatus.Declined }.ToFrozenSet(),
            [IncidentStatus.Verifying] = new[] { IncidentStatus.Resolved, IncidentStatus.InProgress }.ToFrozenSet(),
            [IncidentStatus.Resolved] = new[] { IncidentStatus.InProgress }.ToFrozenSet(),
            [IncidentStatus.Declined] = new[] { IncidentStatus.InProgress }.ToFrozenSet()
        }.ToFrozenDictionary();

    private Incident() { }

    public static Incident Create(string title, string description, IncidentPriority priority, ChangeContext context)
    {
        var incident = new Incident
        {
            PublicId = Guid.NewGuid(),
            Title = title,
            Description = description,
            Priority = priority,
            Status = IncidentStatus.Open
        };

        incident.Creation = new IncidentCreation(incident, context.AtUtc);
        return incident;
    }

    public int IncidentId { get; private set; }

    public Guid PublicId { get; private set; }

    public int TitleRevision { get; private set; }

    public int DescriptionRevision { get; private set; }

    public int PriorityRevision { get; private set; }

    public int StatusRevision { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IncidentStatus Status { get; private set; }

    public IncidentPriority Priority { get; private set; } = IncidentPriority.Medium;

    public IncidentCreation Creation { get; private set; } = null!;

    public List<IncidentStatusChange> StatusHistory { get; private set; } = [];

    public List<IncidentTitleChange> TitleHistory { get; private set; } = [];

    public List<IncidentDescriptionChange> DescriptionHistory { get; private set; } = [];

    public List<IncidentPriorityChange> PriorityHistory { get; private set; } = [];

    public bool UpdateTitle(string newTitle, ChangeContext context)
    {
        if (Title == newTitle)
        {
            return false;
        }

        TitleHistory.Add(new IncidentTitleChange(this, Title, newTitle, context.AtUtc));
        TitleRevision++;
        Title = newTitle;
        return true;
    }

    public bool UpdateDescription(string newDescription, ChangeContext context)
    {
        if (Description == newDescription)
        {
            return false;
        }

        DescriptionHistory.Add(new IncidentDescriptionChange(this, Description, newDescription, context.AtUtc));
        DescriptionRevision++;
        Description = newDescription;
        return true;
    }

    public bool UpdatePriority(IncidentPriority newPriority, ChangeContext context)
    {
        if (Priority == newPriority)
        {
            return false;
        }

        PriorityHistory.Add(new IncidentPriorityChange(this, Priority, newPriority, context.AtUtc));
        PriorityRevision++;
        Priority = newPriority;
        return true;
    }

    public void TransitionTo(IncidentStatus next, ChangeContext context)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(next))
        {
            throw new InvalidOperationException($"Invalid transition: {Status} -> {next}.");
        }

        StatusHistory.Add(new IncidentStatusChange(this, Status, next, context.AtUtc));
        StatusRevision++;
        Status = next;
    }
}
