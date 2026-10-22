namespace incidenskezelo_backend.Services.AI;

public interface IIncidentAnalysisService
{
    Task<string> AnalyzeAsync(
        Guid incidentId,
        CancellationToken cancellationToken = default);
}
