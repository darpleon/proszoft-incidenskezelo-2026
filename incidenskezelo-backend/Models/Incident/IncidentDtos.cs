using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Models.Incident;

public record CreateIncidentRequest(string Title, string Description, IncidentPriority Priority);

public record TransitionRequest(IncidentStatus Status);

public record UpdateTitleRequest(string Title);

public record UpdateDescriptionRequest(string Description);

public record UpdatePriorityRequest(IncidentPriority Priority);

public record IncidentResponse(
    Guid Id,
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
        DateTime? closedAt = incident.Status is IncidentStatus.Resolved or IncidentStatus.Declined
            ? incident.StatusHistory.Last(c => c.NewValue == incident.Status).OccurredAtUtc
            : null;

        return new IncidentResponse(
            incident.PublicId,
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
        IEnumerable<IncidentChange>[] streams =
        [
            incident.StatusHistory,
            incident.TitleHistory,
            incident.DescriptionHistory,
            incident.PriorityHistory
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
    string? NewValue);
