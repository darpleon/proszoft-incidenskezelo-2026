namespace incidenskezelo_backend.Models.Incident.Changes;

public abstract class IncidentChange
{
    public int Id { get; private set; }

    public int IncidentId { get; private set; }

    public Incident Incident { get; private set; } = null!;

    public DateTime OccurredAtUtc { get; private set; }

    protected IncidentChange() { }

    protected IncidentChange(Incident incident, DateTime at)
    {
        Incident = incident;
        OccurredAtUtc = at;
    }
}

public abstract class IncidentChange<T> : IncidentChange
{
    public T OldValue { get; private set; } = default!;

    public T NewValue { get; private set; } = default!;

    protected IncidentChange() { }

    protected IncidentChange(Incident incident, T old, T @new, DateTime at)
        : base(incident, at)
    {
        OldValue = old;
        NewValue = @new;
    }
}
