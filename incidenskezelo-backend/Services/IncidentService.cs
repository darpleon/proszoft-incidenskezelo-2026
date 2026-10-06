using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace incidenskezelo_backend.Services;

public class IncidentService(AppDbContext context) : IIncidentService
{
    public async Task<List<Incident>> GetAllAsync(CancellationToken ct)
    {
        return await context.Incidents.AsNoTracking().ToListAsync(ct);
    }

    public async Task<Incident?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Incidents.AsNoTracking()
            .FirstOrDefaultAsync(i => i.IncidentId == id, ct);
    }

    public async Task<Incident> CreateAsync(CreateIncidentRequest request, CancellationToken ct)
    {
        var incident = Incident.Create(request.Title, request.Description, request.Priority);
        context.Incidents.Add(incident);
        await context.SaveChangesAsync(ct);
        return incident;
    }

    public async Task<Incident?> TransitionAsync(int id, IncidentStatus status, CancellationToken ct)
    {
        var incident = await context.Incidents
            .FirstOrDefaultAsync(i => i.IncidentId == id, ct);

        if (incident is null)
        {
            return null;
        }

        incident.TransitionTo(status);
        await context.SaveChangesAsync(ct);
        return incident;
    }
}
