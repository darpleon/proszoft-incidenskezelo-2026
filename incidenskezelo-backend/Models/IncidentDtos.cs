namespace incidenskezelo_backend.Models;

public record CreateIncidentRequest(string Title, string Description, IncidentPriority Priority);

public record TransitionRequest(IncidentStatus Status);

public record IncidentResponse(
    int IncidentId,
    string Title,
    string Description,
    IncidentStatus Status,
    IncidentPriority Priority,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc)
{
    public static IncidentResponse From(Incident incident)
    {
        var updatedAt = incident.StatusHistory.Count == 0
            ? incident.CreatedAtUtc
            : incident.StatusHistory.Max(c => c.OccurredAtUtc);

        return new IncidentResponse(
            incident.IncidentId,
            incident.Title,
            incident.Description,
            incident.Status,
            incident.Priority,
            incident.CreatedAtUtc,
            updatedAt);
    }
}

public record IncidentStatusChangeResponse(
    int Id,
    int IncidentId,
    IncidentStatus? FromStatus,
    IncidentStatus ToStatus,
    DateTime OccurredAtUtc)
{
    public static IncidentStatusChangeResponse From(IncidentStatusChange change)
    {
        return new IncidentStatusChangeResponse(
            change.Id,
            change.IncidentId,
            change.FromStatus,
            change.ToStatus,
            change.OccurredAtUtc);
    }
}
