using incidenskezelo_backend.Models.Incident;

namespace incidenskezelo_backend.Services;

public sealed class RevisionConflictException(string field, int currentRevision, string currentValue, IncidentRevisions current)
    : Exception("The incident has been modified since it was loaded.")
{
    public string Field { get; } = field;

    public int CurrentRevision { get; } = currentRevision;

    public string CurrentValue { get; } = currentValue;

    public IncidentRevisions Current { get; } = current;
}
