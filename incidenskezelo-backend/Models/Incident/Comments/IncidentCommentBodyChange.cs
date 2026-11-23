using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Models.Incident.Comments;

public class IncidentCommentBodyChange
{
    private IncidentCommentBodyChange() { }

    internal IncidentCommentBodyChange(IncidentComment comment, string oldBody, string newBody, ChangeContext context)
    {
        Comment = comment;
        OldBody = oldBody;
        NewBody = newBody;
        OccurredAtUtc = context.AtUtc;
        ActorUserId = context.ActorUserId;
    }

    public int ChangeId { get; private set; }

    public int CommentId { get; private set; }

    public IncidentComment Comment { get; private set; } = null!;

    public DateTime OccurredAtUtc { get; private set; }

    public int? ActorUserId { get; private set; }

    public User? Actor { get; private set; }

    public string OldBody { get; private set; } = string.Empty;

    public string NewBody { get; private set; } = string.Empty;
}
