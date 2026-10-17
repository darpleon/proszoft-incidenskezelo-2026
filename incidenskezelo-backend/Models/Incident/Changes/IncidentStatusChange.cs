using incidenskezelo_backend.Models.Incident;

namespace incidenskezelo_backend.Models.Incident.Changes;

public class IncidentStatusChange
{
    public int Id { get; private set; }

    public int IncidentId { get; private set; }

    public Incident Incident { get; private set; } = null!;

    public IncidentStatus? FromStatus { get; private set; }

    public IncidentStatus ToStatus { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    private IncidentStatusChange() { }

    internal IncidentStatusChange(Incident incident, IncidentStatus? from, IncidentStatus to, DateTime at)
    {
        Incident = incident;
        FromStatus = from;
        ToStatus = to;
        OccurredAtUtc = at;
    }
}
