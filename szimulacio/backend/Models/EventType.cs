namespace backend.Models;

public enum EventType
{
    ServiceDown,
    ServiceRecovered,
    HighLatency,
    HighErrorRate,
    DbConnectionError,
    JobFailed,
    CapacityIssue,
    DependencyFailure,
}
