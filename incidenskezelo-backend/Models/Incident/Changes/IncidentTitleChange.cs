namespace incidenskezelo_backend.Models.Incident.Changes;

public class IncidentTitleChange : IncidentChange<string>
{
    private IncidentTitleChange() { }

    internal IncidentTitleChange(Incident incident, string oldTitle, string newTitle, DateTime at)
        : base(incident, oldTitle, newTitle, at)
    {
    }
}
