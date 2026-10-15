namespace incidenskezelo_backend.Models.Incident.Changes;

public class IncidentPriorityChange : IncidentChange<IncidentPriority>
{
    private IncidentPriorityChange() { }

    internal IncidentPriorityChange(Incident incident, IncidentPriority oldPriority, IncidentPriority newPriority, DateTime at)
        : base(incident, oldPriority, newPriority, at)
    {
    }
}
