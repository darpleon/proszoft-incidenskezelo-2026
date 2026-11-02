namespace incidenskezelo_backend.Models.Incident.Changes;

public abstract class IncidentChange
{
    public int Id { get; private set; }

    public int IncidentId { get; private set; }

    public Incident Incident { get; private set; } = null!;

    public DateTime OccurredAtUtc { get; private set; }

    public int? ActorUserId { get; private set; }

    public User? Actor { get; private set; }

    protected IncidentChange() { }

    protected IncidentChange(Incident incident, ChangeContext context)
    {
        Incident = incident;
        OccurredAtUtc = context.AtUtc;
        ActorUserId = context.ActorUserId;
    }
}

public abstract class IncidentChange<T> : IncidentChange
{
    public T OldValue { get; private set; } = default!;

    public T NewValue { get; private set; } = default!;

    protected IncidentChange() { }

    protected IncidentChange(Incident incident, T old, T @new, ChangeContext context)
        : base(incident, context)
    {
        OldValue = old;
        NewValue = @new;
    }
}
