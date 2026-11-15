using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Models.Incident;

public class IncidentComment
{
    private IncidentComment() { }

    internal IncidentComment(Incident incident, string text, ChangeContext context)
    {
        Incident = incident;
        Text = text;
        OccurredAtUtc = context.AtUtc;
        AuthorUserId = context.ActorUserId;
    }

    public int CommentId { get; private set; }

    public int IncidentId { get; private set; }

    public Incident Incident { get; private set; } = null!;

    public int? AuthorUserId { get; private set; }

    public User? Author { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public DateTime OccurredAtUtc { get; private set; }
}
