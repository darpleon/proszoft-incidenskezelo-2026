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
            .Include(i => i.Creation)
            .Include(i => i.StatusHistory)
            .Include(i => i.TitleHistory)
            .Include(i => i.DescriptionHistory)
            .Include(i => i.PriorityHistory)
            .OrderByDescending(i => i.Creation.OccurredAtUtc)
            .Select(i => IncidentResponse.From(i))
            .ToListAsync(ct);
    }

    public async Task<IncidentResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Incidents.AsNoTracking()
            .Where(i => i.IncidentId == id)
            .Include(i => i.Creation)
            .Include(i => i.StatusHistory)
            .Include(i => i.TitleHistory)
            .Include(i => i.DescriptionHistory)
            .Include(i => i.PriorityHistory)
            .Select(i => IncidentResponse.From(i))
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
        var incident = await LoadWithHistoryAsync(id, ct);

        if (incident is null)
        {
            return null;
        }

        incident.TransitionTo(status, new ChangeContext(timeProvider.GetUtcNow().UtcDateTime));
        await context.SaveChangesAsync(ct);

        return IncidentResponse.From(incident);
    }

    public Task<IncidentResponse?> UpdateTitleAsync(int id, string title, CancellationToken ct)
    {
        return ApplyAsync(id, (incident, context) => incident.UpdateTitle(title, context), ct);
    }

    public Task<IncidentResponse?> UpdateDescriptionAsync(int id, string description, CancellationToken ct)
    {
        return ApplyAsync(id, (incident, context) => incident.UpdateDescription(description, context), ct);
    }

    public Task<IncidentResponse?> UpdatePriorityAsync(int id, IncidentPriority priority, CancellationToken ct)
    {
        return ApplyAsync(id, (incident, context) => incident.UpdatePriority(priority, context), ct);
    }

    private async Task<IncidentResponse?> ApplyAsync(int id, Action<Incident, ChangeContext> update, CancellationToken ct)
    {
        var incident = await LoadWithHistoryAsync(id, ct);

        if (incident is null)
        {
            return null;
        }

        update(incident, new ChangeContext(timeProvider.GetUtcNow().UtcDateTime));
        await context.SaveChangesAsync(ct);

        return IncidentResponse.From(incident);
    }

    private Task<Incident?> LoadWithHistoryAsync(int id, CancellationToken ct)
    {
        return context.Incidents
            .Include(i => i.Creation)
            .Include(i => i.StatusHistory)
            .Include(i => i.TitleHistory)
            .Include(i => i.DescriptionHistory)
            .Include(i => i.PriorityHistory)
            .FirstOrDefaultAsync(i => i.IncidentId == id, ct);
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

}
