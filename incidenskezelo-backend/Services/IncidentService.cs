using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models.Incident;
using incidenskezelo_backend.Models.Incident.Changes;
using Microsoft.EntityFrameworkCore;

namespace incidenskezelo_backend.Services;

public class IncidentService(AppDbContext context, TimeProvider timeProvider) : IIncidentService
{
    public async Task<List<IncidentResponse>> GetAllAsync(CancellationToken ct)
    {
        return await context.Incidents.AsNoTracking()
            .OrderByDescending(i => i.Creation.OccurredAtUtc)
            .Include(i => i.StatusHistory)
            .Include(i => i.Creation)
            .Select(i => ToResponse(i))
            .ToListAsync(ct);
    }

    public async Task<IncidentResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Incidents.AsNoTracking()
            .Where(i => i.IncidentId == id)
            .Include(i => i.StatusHistory)
            .Include(i => i.Creation)
            .Select(i => ToResponse(i))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IncidentResponse> CreateAsync(CreateIncidentRequest request, CancellationToken ct)
    {
        var changeContext = new ChangeContext(timeProvider.GetUtcNow().UtcDateTime);

        var incident = Incident.Create(request.Title, request.Description, request.Priority, changeContext);
        context.Incidents.Add(incident);
        await context.SaveChangesAsync(ct);

        return IncidentResponse.From(incident);
    }

    public async Task<IncidentResponse?> TransitionAsync(int id, IncidentStatus status, CancellationToken ct)
    {
        var incident = await context.Incidents
            .Include(i => i.StatusHistory)
            .Include(i => i.Creation)
            .FirstOrDefaultAsync(i => i.IncidentId == id, ct);

        if (incident is null)
        {
            return null;
        }

        var changeContext = new ChangeContext(timeProvider.GetUtcNow().UtcDateTime);
        incident.TransitionTo(status, changeContext);
        await context.SaveChangesAsync(ct);

        return IncidentResponse.From(incident);
    }

    public async Task<List<IncidentStatusChangeResponse>?> GetHistoryAsync(int id, CancellationToken ct)
    {
        var exists = await context.Incidents.AsNoTracking()
            .AnyAsync(i => i.IncidentId == id, ct);

        if (!exists)
        {
            return null;
        }

        return await context.IncidentStatusChanges.AsNoTracking()
            .Where(c => c.IncidentId == id)
            .OrderBy(c => c.OccurredAtUtc)
            .ThenBy(c => c.Id)
            .Select(c => new IncidentStatusChangeResponse(
                c.Id, c.IncidentId, c.OldValue, c.NewValue, c.OccurredAtUtc))
            .ToListAsync(ct);
    }

    private static IncidentResponse ToResponse(Incident i)
    {
        return new IncidentResponse(
            i.IncidentId,
            i.Title,
            i.Description,
            i.Status,
            i.Priority,
            i.Creation.OccurredAtUtc,
            i.StatusHistory.Max(c => (DateTime?)c.OccurredAtUtc) ?? i.Creation.OccurredAtUtc,
            i.StatusHistory
                .Where(c => c.NewValue == IncidentStatus.Resolved)
                .Select(c => (DateTime?)c.OccurredAtUtc)
                .FirstOrDefault());
    }
}
