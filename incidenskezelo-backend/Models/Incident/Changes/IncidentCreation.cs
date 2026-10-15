namespace incidenskezelo_backend.Models.Incident.Changes;

public class IncidentCreation
{
    public int Id { get; private set; }

    public int IncidentId { get; private set; }

    public Incident Incident { get; private set; } = null!;

    public DateTime OccurredAtUtc { get; private set; }

    private IncidentCreation() { }

    internal IncidentCreation(Incident incident, DateTime at)
    {
        Incident = incident;
        OccurredAtUtc = at;
    }
}
