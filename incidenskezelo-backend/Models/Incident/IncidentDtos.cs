using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Models.Incident;

public record CreateIncidentRequest(string Title, string Description, IncidentPriority Priority);

public record TransitionRequest(IncidentStatus Status);

public record UpdateTitleRequest(string Title);

public record UpdateDescriptionRequest(string Description);

public record UpdatePriorityRequest(IncidentPriority Priority);

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
            LatestChangeUtc(incident),
            closedAt);
    }

    private static DateTime LatestChangeUtc(Incident incident)
    {
        return incident.StatusHistory.Select(c => c.OccurredAtUtc)
            .Concat(incident.TitleHistory.Select(c => c.OccurredAtUtc))
            .Concat(incident.DescriptionHistory.Select(c => c.OccurredAtUtc))
            .Concat(incident.PriorityHistory.Select(c => c.OccurredAtUtc))
            .Append(incident.Creation.OccurredAtUtc)
            .Max();
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
