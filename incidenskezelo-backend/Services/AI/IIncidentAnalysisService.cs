using incidenskezelo_backend.Models.AI;

namespace incidenskezelo_backend.Services.AI;

public interface IIncidentAnalysisService
{
    Task<IncidentAnalysisResult> AnalyzeAsync(
        Guid incidentId,
        AiProvider provider,
        CancellationToken cancellationToken = default);

    Task<StoredIncidentAnalysis?> GetLatestAsync(
        Guid incidentId,
        CancellationToken cancellationToken);
}
