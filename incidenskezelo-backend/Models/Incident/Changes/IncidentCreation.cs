namespace incidenskezelo_backend.Models.Incident.Changes;

public class IncidentCreation : IncidentChange
{
    private IncidentCreation() { }

    internal IncidentCreation(Incident incident, DateTime at)
        : base(incident, at)
    {
    }
}
