using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Models.Incident;

public record CreateIncidentRequest(string Title, string Description, IncidentPriority Priority);

public record TransitionRequest(IncidentStatus Status, int? ExpectedRevision);

public record UpdateTitleRequest(string Title, int? ExpectedRevision);

public record UpdateDescriptionRequest(string Description, int? ExpectedRevision);

public record UpdatePriorityRequest(IncidentPriority Priority, int? ExpectedRevision);

public record UpdateAssigneeRequest(int? AssigneeUserId, int? ExpectedRevision);

public record IncidentRevisions(int Title, int Description, int Priority, int Status, int Assignee)
{
    public static IncidentRevisions From(Incident incident) =>
        new(incident.TitleRevision, incident.DescriptionRevision, incident.PriorityRevision, incident.StatusRevision, incident.AssigneeRevision);
}

public record IncidentResponse(
    Guid Id,
    string Title,
    string Description,
    IncidentStatus Status,
    IncidentPriority Priority,
    int? AssigneeUserId,
    string? AssigneeName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? ClosedAtUtc,
    IncidentRevisions Revisions)
{
    public static IncidentResponse From(Incident incident)
    {
        DateTime? closedAt = incident.Status is IncidentStatus.Resolved or IncidentStatus.Declined
            ? incident.StatusHistory.Last(c => c.NewValue == incident.Status).OccurredAtUtc
            : null;

        return new IncidentResponse(
            incident.PublicId,
            incident.Title,
            incident.Description,
            incident.Status,
            incident.Priority,
            incident.AssigneeUserId,
            incident.Assignee?.Name,
            incident.Creation.OccurredAtUtc,
            LatestChangeUtc(incident),
            closedAt,
            IncidentRevisions.From(incident));
    }

    private static DateTime LatestChangeUtc(Incident incident)
    {
        IEnumerable<IncidentChange>[] streams =
        [
            incident.StatusHistory,
            incident.TitleHistory,
            incident.DescriptionHistory,
            incident.PriorityHistory,
            incident.AssigneeHistory
        ];

        return streams.SelectMany(s => s)
            .Select(c => c.OccurredAtUtc)
            .Append(incident.Creation.OccurredAtUtc)
            .Max();
    }
}

public record IncidentChangeResponse(
    string Kind,
    DateTime OccurredAtUtc,
    string? OldValue,
    string? NewValue,
    int? ActorUserId,
    string? ActorName);
