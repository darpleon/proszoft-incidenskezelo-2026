namespace incidenskezelo_backend.Models.Incident.Changes;

public class IncidentStatusChange : IncidentChange<IncidentStatus>
{
    private IncidentStatusChange() { }

    internal IncidentStatusChange(Incident incident, IncidentStatus from, IncidentStatus to, DateTime at)
        : base(incident, from, to, at)
    {
    }
}
