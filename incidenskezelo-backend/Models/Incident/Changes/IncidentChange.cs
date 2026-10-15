namespace incidenskezelo_backend.Models.Incident.Changes;

public abstract class IncidentChange<T>
{
    public int Id { get; private set; }

    public int IncidentId { get; private set; }

    public Incident Incident { get; private set; } = null!;

    public T OldValue { get; private set; } = default!;

    public T NewValue { get; private set; } = default!;

    public DateTime OccurredAtUtc { get; private set; }

    protected IncidentChange() { }

    protected IncidentChange(Incident incident, T old, T @new, DateTime at)
    {
        Incident = incident;
        OldValue = old;
        NewValue = @new;
        OccurredAtUtc = at;
    }
}
