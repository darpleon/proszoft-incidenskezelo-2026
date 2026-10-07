using incidenskezelo_backend.Models;

namespace incidenskezelo_backend.Services;

public interface IIncidentService
{
    Task<List<IncidentResponse>> GetAllAsync(CancellationToken ct);
    Task<IncidentResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<IncidentResponse> CreateAsync(CreateIncidentRequest request, CancellationToken ct);
    Task<IncidentResponse?> TransitionAsync(int id, IncidentStatus status, CancellationToken ct);
    Task<List<IncidentStatusChangeResponse>?> GetHistoryAsync(int id, CancellationToken ct);
}
