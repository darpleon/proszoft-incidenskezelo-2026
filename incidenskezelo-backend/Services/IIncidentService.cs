using incidenskezelo_backend.Models;

namespace incidenskezelo_backend.Services;

public interface IIncidentService
{
    Task<List<Incident>> GetAllAsync(CancellationToken ct);
    Task<Incident?> GetByIdAsync(int id, CancellationToken ct);
    Task<Incident> CreateAsync(CreateIncidentRequest request, CancellationToken ct);
    Task<Incident?> TransitionAsync(int id, IncidentStatus status, CancellationToken ct);
}
