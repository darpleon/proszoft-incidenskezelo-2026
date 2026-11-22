namespace incidenskezelo_backend.Services;

public sealed class CommentCreatorOnlyException(int commentIndex)
    : Exception("Only the comment's creator can edit it.")
{
    public int CommentIndex { get; } = commentIndex;
}
