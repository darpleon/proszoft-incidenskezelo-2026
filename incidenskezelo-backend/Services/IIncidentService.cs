using incidenskezelo_backend.Models.Incident;
using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Services;

public interface IIncidentService
{
    Task<List<IncidentResponse>> GetAllAsync(CancellationToken ct);
    Task<IncidentResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<IncidentResponse> CreateAsync(CreateIncidentRequest request, CancellationToken ct);
    Task<IncidentResponse?> TransitionAsync(int id, IncidentStatus status, CancellationToken ct);
    Task<IncidentResponse?> UpdateTitleAsync(int id, string title, CancellationToken ct);
    Task<IncidentResponse?> UpdateDescriptionAsync(int id, string description, CancellationToken ct);
    Task<IncidentResponse?> UpdatePriorityAsync(int id, IncidentPriority priority, CancellationToken ct);
    Task<List<IncidentStatusChangeResponse>?> GetHistoryAsync(int id, CancellationToken ct);
}
