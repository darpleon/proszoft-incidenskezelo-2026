using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using incidenskezelo_backend.Models.Incident;
using Microsoft.EntityFrameworkCore;

namespace incidenskezelo_backend.Services.IncidentAutomation;

public enum IncidentCandidateStatus
{
    None,
    Unique,
    Ambiguous
}

public sealed record IncidentCandidateResult(
    IncidentCandidateStatus Status,
    Incident? Incident,
    string? MatchRule);

public sealed class IncidentCorrelationService(AppDbContext dbContext)
{
    public async Task<IncidentCandidateResult> FindCandidateAsync(
        Event incomingEvent,
        CancellationToken cancellationToken)
    {
        // A Resolved és Declined státuszú incidentek kézzel lezártak
        // Ezekhez normál új eventet nem kapcsolunk
        var openIncidents = await dbContext.Incidents
            .Where(incident =>
                incident.Status != IncidentStatus.Resolved &&
                incident.Status != IncidentStatus.Declined)
            .Include(incident => incident.Events)
            .ToListAsync(cancellationToken);

        var correlationKey = incomingEvent.CorrelationKey?.Trim();

        // Ha a küldő adott CorrelationKey-t, az az elsődleges csoportosítási jel
        // Eltérő kulcs esetén nem kapcsolunk gyengébb szabály alapján
        if (!string.IsNullOrWhiteSpace(correlationKey))
        {
            var keyMatches = openIncidents.Where(incident =>
                incident.Events.Any(existingEvent =>
                    string.Equals(
                        existingEvent.CorrelationKey?.Trim(),
                        correlationKey,
                        StringComparison.OrdinalIgnoreCase)));

            return SelectUnique(keyMatches, "CorrelationKey");
        }

        // Kulcs nélkül először a payloadból normalizált függőséget vizsgáljuk
        var dependencyCode =
            DependencyNormalizer.FromPayload(incomingEvent.PayloadJson);

        if (dependencyCode is not null)
        {
            var dependencyMatches = openIncidents.Where(incident =>
                incident.Events.Any(existingEvent =>
                    string.Equals(
                        DependencyNormalizer.FromPayload(existingEvent.PayloadJson),
                        dependencyCode,
                        StringComparison.OrdinalIgnoreCase)));

            var dependencyResult =
                SelectUnique(dependencyMatches, "Dependency");

            if (dependencyResult.Status != IncidentCandidateStatus.None)
            {
                return dependencyResult;
            }
        }

        var classification =
            IncidentCorrelationRules.Classify(incomingEvent.EventType);

        // Azonos service + hibacsalád alapján keresünk
        if (classification.Family is { } family)
        {
            var familyMatches = openIncidents.Where(incident =>
                incident.Events.Any(existingEvent =>
                    existingEvent.ServiceId == incomingEvent.ServiceId &&
                    IncidentCorrelationRules
                        .Classify(existingEvent.EventType)
                        .Family == family));

            var familyResult =
                SelectUnique(familyMatches, "ServiceAndFamily");

            if (familyResult.Status != IncidentCandidateStatus.None)
            {
                return familyResult;
            }
        }

        // A ServiceRecovered nem indít incidentet
        // Kulcs és függőségi adat nélkül csak akkor kapcsolható,
        // ha pontosan egy nyitott incident van ugyanezen a service-en
        if (classification.Role == IncidentEventRole.Recovery)
        {
            var serviceMatches = openIncidents.Where(incident =>
                incident.Events.Any(existingEvent =>
                    existingEvent.ServiceId == incomingEvent.ServiceId));

            return SelectUnique(serviceMatches, "UniqueIncidentForService");
        }

        return new IncidentCandidateResult(
            IncidentCandidateStatus.None,
            null,
            null);
    }

    private static IncidentCandidateResult SelectUnique(
        IEnumerable<Incident> candidates,
        string matchRule)
    {
        var distinctCandidates = candidates
            .DistinctBy(incident => incident.IncidentId)
            .ToList();

        return distinctCandidates.Count switch
        {
            0 => new IncidentCandidateResult(
                IncidentCandidateStatus.None,
                null,
                matchRule),

            1 => new IncidentCandidateResult(
                IncidentCandidateStatus.Unique,
                distinctCandidates[0],
                matchRule),

            _ => new IncidentCandidateResult(
                IncidentCandidateStatus.Ambiguous,
                null,
                matchRule)
        };
    }
}