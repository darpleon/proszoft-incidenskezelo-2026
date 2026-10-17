using incidenskezelo_backend.Models.Incident;
using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Services;

public interface IIncidentService
{
    Task<List<IncidentResponse>> GetAllAsync(CancellationToken ct);
    Task<IncidentResponse?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IncidentResponse> CreateAsync(CreateIncidentRequest request, CancellationToken ct);
    Task<IncidentResponse?> TransitionAsync(Guid id, IncidentStatus status, int expectedRevision, CancellationToken ct);
    Task<IncidentResponse?> UpdateTitleAsync(Guid id, string title, int expectedRevision, CancellationToken ct);
    Task<IncidentResponse?> UpdateDescriptionAsync(Guid id, string description, int expectedRevision, CancellationToken ct);
    Task<IncidentResponse?> UpdatePriorityAsync(Guid id, IncidentPriority priority, int expectedRevision, CancellationToken ct);
    Task<List<IncidentChangeResponse>?> GetHistoryAsync(Guid id, CancellationToken ct);
}
