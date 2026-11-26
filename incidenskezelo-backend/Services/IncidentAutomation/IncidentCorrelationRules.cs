using incidenskezelo_backend.Models;
using incidenskezelo_backend.Models.Incident;

namespace incidenskezelo_backend.Services.IncidentAutomation;

public enum IncidentEventRole
{
    Trigger,
    Recovery
}

public enum IncidentFamily
{
    ServiceDegradation,
    DatabaseConnectivity,
    JobExecution,
    DependencyFailure
}

public sealed record IncidentEventClassification(
    IncidentFamily? Family,
    IncidentEventRole Role);

public static class IncidentCorrelationRules
{
    public static IncidentEventClassification Classify(EventType eventType) =>
        eventType switch
        {
            EventType.HighLatency =>
                new(IncidentFamily.ServiceDegradation, IncidentEventRole.Trigger),

            EventType.HighErrorRate =>
                new(IncidentFamily.ServiceDegradation, IncidentEventRole.Trigger),

            EventType.CapacityIssue =>
                new(IncidentFamily.ServiceDegradation, IncidentEventRole.Trigger),

            EventType.ServiceDown =>
                new(IncidentFamily.ServiceDegradation, IncidentEventRole.Trigger),

            EventType.DbConnectionError =>
                new(IncidentFamily.DatabaseConnectivity, IncidentEventRole.Trigger),

            EventType.JobFailed =>
                new(IncidentFamily.JobExecution, IncidentEventRole.Trigger),

            EventType.DependencyFailure =>
                new(IncidentFamily.DependencyFailure, IncidentEventRole.Trigger),

            EventType.ServiceRecovered =>
                new(null, IncidentEventRole.Recovery),

            _ => throw new ArgumentOutOfRangeException(
                nameof(eventType),
                eventType,
                "Unsupported event type.")
        };

    public static IncidentPriority ToPriority(Severity severity) =>
        severity switch
        {
            Severity.Info => IncidentPriority.Low,
            Severity.Warning => IncidentPriority.Medium,
            Severity.Error => IncidentPriority.High,
            Severity.Critical => IncidentPriority.Critical,
            _ => throw new ArgumentOutOfRangeException(
                nameof(severity),
                severity,
                "Unsupported event severity.")
        };
}