using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Models.Incident.Comments;

public class IncidentCommentCreation
{
    private IncidentCommentCreation() { }

    internal IncidentCommentCreation(IncidentComment comment, ChangeContext context)
    {
        Comment = comment;
        OccurredAtUtc = context.AtUtc;
        ActorUserId = context.ActorUserId;
    }

    public int CreationId { get; private set; }

    public int CommentId { get; private set; }

    public IncidentComment Comment { get; private set; } = null!;

    public DateTime OccurredAtUtc { get; private set; }

    public int? ActorUserId { get; private set; }

    public User? Actor { get; private set; }
}
