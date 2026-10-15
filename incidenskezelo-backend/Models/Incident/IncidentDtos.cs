using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Models.Incident;

public record CreateIncidentRequest(string Title, string Description, IncidentPriority Priority);

public record TransitionRequest(IncidentStatus Status);

public record IncidentResponse(
    int IncidentId,
    string Title,
    string Description,
    IncidentStatus Status,
    IncidentPriority Priority,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? ClosedAtUtc)
{
    public static IncidentResponse From(Incident incident)
    {
        var updatedAt = incident.StatusHistory.Count == 0
            ? incident.Creation.OccurredAtUtc
            : incident.StatusHistory.Max(c => c.OccurredAtUtc);

        var closedAt = incident.StatusHistory
            .Where(c => c.NewValue == IncidentStatus.Resolved)
            .Select(c => (DateTime?)c.OccurredAtUtc)
            .FirstOrDefault();

        return new IncidentResponse(
            incident.IncidentId,
            incident.Title,
            incident.Description,
            incident.Status,
            incident.Priority,
            incident.Creation.OccurredAtUtc,
            updatedAt,
            closedAt);
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
            change.OldValue,
            change.NewValue,
            change.OccurredAtUtc);
    }
}
