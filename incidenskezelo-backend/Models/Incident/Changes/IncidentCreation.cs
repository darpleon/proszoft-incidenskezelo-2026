namespace incidenskezelo_backend.Models.Incident.Changes;

public class IncidentCreation : IncidentChange
{
    private IncidentCreation() { }

    internal IncidentCreation(Incident incident, ChangeContext context)
        : base(incident, context)
    {
    }
}
