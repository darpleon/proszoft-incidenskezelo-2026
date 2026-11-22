using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Models.Incident.Comments;

public class IncidentComment
{
    private IncidentComment() { }

    internal IncidentComment(Incident incident, int commentIndex, string body, ChangeContext context)
    {
        Incident = incident;
        CommentIndex = commentIndex;
        Body = body;
        Creation = new IncidentCommentCreation(this, context);
    }

    public int CommentId { get; private set; }

    public int IncidentId { get; private set; }

    public Incident Incident { get; private set; } = null!;

    public IncidentCommentCreation Creation { get; private set; } = null!;

    public int CommentIndex { get; private set; }

    public string Body { get; private set; } = string.Empty;

    public int BodyRevision { get; private set; }

    public List<IncidentCommentBodyChange> BodyHistory { get; private set; } = [];

    public bool UpdateBody(string newBody, ChangeContext context)
    {
        if (Body == newBody)
        {
            return false;
        }

        BodyHistory.Add(new IncidentCommentBodyChange(this, Body, newBody, context));
        BodyRevision++;
        Body = newBody;
        return true;
    }
}
