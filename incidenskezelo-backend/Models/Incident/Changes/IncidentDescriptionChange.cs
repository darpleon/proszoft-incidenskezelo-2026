namespace incidenskezelo_backend.Models.Incident.Changes;

public class IncidentDescriptionChange : IncidentChange<string>
{
    private IncidentDescriptionChange() { }

    internal IncidentDescriptionChange(Incident incident, string oldDescription, string newDescription, DateTime at)
        : base(incident, oldDescription, newDescription, at)
    {
    }
}
