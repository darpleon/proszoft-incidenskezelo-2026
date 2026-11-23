namespace incidenskezelo_backend.Services;

public sealed class CommentRevisionConflictException(int commentIndex, int currentRevision, string currentValue)
    : Exception("The comment has been modified since it was loaded.")
{
    public int CommentIndex { get; } = commentIndex;

    public int CurrentRevision { get; } = currentRevision;

    public string CurrentValue { get; } = currentValue;
}
