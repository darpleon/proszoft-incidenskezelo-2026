namespace incidenskezelo_backend.Models.Incident.Changes;

public class IncidentEventLink : IncidentChange
{
    private IncidentEventLink() { }

    internal IncidentEventLink(Incident incident, long eventId, ChangeContext context)
        : base(incident, context)
    {
        EventId = eventId;
    }

    public long EventId { get; private set; }
}
