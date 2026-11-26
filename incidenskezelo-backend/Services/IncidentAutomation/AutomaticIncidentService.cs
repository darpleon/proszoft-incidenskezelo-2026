using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using incidenskezelo_backend.Models.Incident;
using incidenskezelo_backend.Models.Incident.Changes;
using incidenskezelo_backend.Services.IncidentAutomation;
using System.Globalization;

namespace incidenskezelo_backend.Services.IncidentAutomation;

public sealed class AutomaticIncidentService(
    AppDbContext dbContext,
    TimeProvider timeProvider)
{
    public Incident CreateForEvent(
        Event evt,
        Service service,
        IncidentFamily family)
    {
        var familyName = GetFamilyName(family);
        var summary = string.IsNullOrWhiteSpace(evt.Summary)
            ? "No summary provided."
            : evt.Summary.Trim();

        var title = $"{service.Name} - {familyName}";
        var description =
            "Automatically created from an incoming event.\n\n" +
            $"Affected service: {service.Name} ({service.Code})\n" +
            $"Failure family: {familyName}\n" +
            $"Initial event: {evt.EventType}\n" +
            $"Severity: {evt.Severity}\n" +
            $"Occurred at: {evt.OccurredAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)}\n" +
            $"Summary: {summary}";

        var context = NewContext();

        var incident = Incident.Create(
            title,
            description,
            IncidentCorrelationRules.ToPriority(evt.Severity),
            context);

        // Az eventet előbb el kell menteni, hogy már legyen backend EventId-ja.
        incident.LinkEvents([evt], context);

        dbContext.Incidents.Add(incident);
        return incident;
    }

    public bool AttachEvent(
        Incident incident,
        Event evt)
    {
        // Védelem az azonos event másodszori kapcsolása ellen.
        if (incident.Events.Any(existing => existing.EventId == evt.EventId))
        {
            return false;
        }

        var context = NewContext();

        incident.LinkEvents([evt], context);

        var incomingPriority =
            IncidentCorrelationRules.ToPriority(evt.Severity);

        // A prioritás csak emelkedhet automatikusan.
        if ((int)incomingPriority > (int)incident.Priority)
        {
            incident.UpdatePriority(incomingPriority, context);
        }

        // Szándékosan nincs TransitionTo hívás:
        // automatikusan nem zárunk le incidentet.
        return true;
    }

    private ChangeContext NewContext() =>
        new(timeProvider.GetUtcNow().UtcDateTime, ActorUserId: null);

    private static string GetFamilyName(IncidentFamily family) =>
        family switch
        {
            IncidentFamily.ServiceDegradation => "Service Degradation",
            IncidentFamily.DatabaseConnectivity => "Database Connectivity Failure",
            IncidentFamily.JobExecution => "Background Job Failure",
            IncidentFamily.DependencyFailure => "Dependency Failure",
            _ => throw new ArgumentOutOfRangeException(
                nameof(family),
                family,
                "Unsupported incident family.")
        };
}