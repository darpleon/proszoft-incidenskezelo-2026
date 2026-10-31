using incidenskezelo_backend.Models.AI;

namespace incidenskezelo_backend.Services.AI;

public interface IIncidentAnalysisService
{
    Task<string> AnalyzeAsync(
        Guid incidentId,
        AiProvider provider,
        CancellationToken cancellationToken = default);
}
