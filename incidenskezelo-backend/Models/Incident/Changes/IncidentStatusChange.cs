namespace incidenskezelo_backend.Models.Incident.Changes;

public class IncidentStatusChange : IncidentChange<IncidentStatus>
{
    private IncidentStatusChange() { }

    internal IncidentStatusChange(Incident incident, IncidentStatus from, IncidentStatus to, ChangeContext context)
        : base(incident, from, to, context)
    {
    }
}
