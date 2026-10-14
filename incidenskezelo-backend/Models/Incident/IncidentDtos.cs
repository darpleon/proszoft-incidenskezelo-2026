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
    int Id,
    DateTime OccurredAtUtc,
    string? OldValue,
    string? NewValue);
