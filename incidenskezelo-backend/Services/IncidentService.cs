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

    public async Task<IncidentResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Incidents.AsNoTracking()
            .Where(i => i.PublicId == id)
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

    public async Task<IncidentResponse?> TransitionAsync(Guid id, IncidentStatus status, int expectedRevision, CancellationToken ct)
    {
        var incident = await LoadWithHistoryAsync(id, ct);

        if (incident is null)
        {
            return null;
        }

        DemandRevision(incident, "status", expectedRevision, incident.StatusRevision, incident.Status.ToString());
        incident.TransitionTo(status, new ChangeContext(timeProvider.GetUtcNow().UtcDateTime));
        await context.SaveChangesAsync(ct);

        return IncidentResponse.From(incident);
    }

    public Task<IncidentResponse?> UpdateTitleAsync(Guid id, string title, int expectedRevision, CancellationToken ct)
    {
        return ApplyAsync(id, (incident, context) =>
        {
            DemandRevision(incident, "title", expectedRevision, incident.TitleRevision, incident.Title);
            incident.UpdateTitle(title, context);
        }, ct);
    }

    public Task<IncidentResponse?> UpdateDescriptionAsync(Guid id, string description, int expectedRevision, CancellationToken ct)
    {
        return ApplyAsync(id, (incident, context) =>
        {
            DemandRevision(incident, "description", expectedRevision, incident.DescriptionRevision, incident.Description);
            incident.UpdateDescription(description, context);
        }, ct);
    }

    public Task<IncidentResponse?> UpdatePriorityAsync(Guid id, IncidentPriority priority, int expectedRevision, CancellationToken ct)
    {
        return ApplyAsync(id, (incident, context) =>
        {
            DemandRevision(incident, "priority", expectedRevision, incident.PriorityRevision, incident.Priority.ToString());
            incident.UpdatePriority(priority, context);
        }, ct);
    }

    private async Task<IncidentResponse?> ApplyAsync(Guid id, Action<Incident, ChangeContext> update, CancellationToken ct)
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

    private static void DemandRevision(Incident incident, string field, int expected, int current, string currentValue)
    {
        if (expected != current)
        {
            throw new RevisionConflictException(field, current, currentValue, IncidentRevisions.From(incident));
        }
    }

    private Task<Incident?> LoadWithHistoryAsync(Guid id, CancellationToken ct)
    {
        return context.Incidents
            .Include(i => i.Creation)
            .Include(i => i.StatusHistory)
            .Include(i => i.TitleHistory)
            .Include(i => i.DescriptionHistory)
            .Include(i => i.PriorityHistory)
            .FirstOrDefaultAsync(i => i.PublicId == id, ct);
    }

    public async Task<List<IncidentChangeResponse>?> GetHistoryAsync(Guid id, CancellationToken ct)
    {
        var incidentId = await context.Incidents.AsNoTracking()
            .Where(i => i.PublicId == id)
            .Select(i => (int?)i.IncidentId)
            .FirstOrDefaultAsync(ct);

        if (incidentId is null)
        {
            return null;
        }

        var created = await context.IncidentCreations.AsNoTracking()
            .Where(c => c.IncidentId == incidentId)
            .Select(c => new IncidentChangeResponse("created", c.OccurredAtUtc, null, null))
            .ToListAsync(ct);

        var status = await MapKindAsync(context.IncidentStatusChanges.AsNoTracking().Where(c => c.IncidentId == incidentId), "status", ct);
        var title = await MapKindAsync(context.IncidentTitleChanges.AsNoTracking().Where(c => c.IncidentId == incidentId), "title", ct);
        var description = await MapKindAsync(context.IncidentDescriptionChanges.AsNoTracking().Where(c => c.IncidentId == incidentId), "description", ct);
        var priority = await MapKindAsync(context.IncidentPriorityChanges.AsNoTracking().Where(c => c.IncidentId == incidentId), "priority", ct);

        return created
            .Concat(status)
            .Concat(title)
            .Concat(description)
            .Concat(priority)
            .OrderBy(e => e.OccurredAtUtc)
            .ToList();
    }

    private static async Task<List<IncidentChangeResponse>> MapKindAsync<T>(
        IQueryable<IncidentChange<T>> changes, string kind, CancellationToken ct)
    {
        var rows = await changes
            .OrderBy(c => c.OccurredAtUtc)
            .ThenBy(c => c.Id)
            .Select(c => new { c.OccurredAtUtc, c.OldValue, c.NewValue })
            .ToListAsync(ct);

        return rows
            .Select(r => new IncidentChangeResponse(kind, r.OccurredAtUtc, r.OldValue?.ToString(), r.NewValue?.ToString()))
            .ToList();
    }
}
