using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
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
            .Include(i => i.AssigneeHistory)
            .Include(i => i.Assignee)
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
            .Include(i => i.AssigneeHistory)
            .Include(i => i.Assignee)
            .Select(i => IncidentResponse.From(i))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IncidentResponse> CreateAsync(CreateIncidentRequest request, int actorUserId, CancellationToken ct)
    {
        var changeContext = NewContext(actorUserId);

        var incident = Incident.Create(request.Title, request.Description, request.Priority, changeContext);
        context.Incidents.Add(incident);
        await context.SaveChangesAsync(ct);

        return IncidentResponse.From(incident);
    }

    public async Task<IncidentResponse?> TransitionAsync(Guid id, IncidentStatus status, int expectedRevision, int actorUserId, CancellationToken ct)
    {
        var incident = await LoadWithHistoryAsync(id, ct);

        if (incident is null)
        {
            return null;
        }

        DemandRevision(incident, "status", expectedRevision, incident.StatusRevision, incident.Status.ToString());
        incident.TransitionTo(status, NewContext(actorUserId));
        await context.SaveChangesAsync(ct);

        return IncidentResponse.From(incident);
    }

    public Task<IncidentResponse?> UpdateTitleAsync(Guid id, string title, int expectedRevision, int actorUserId, CancellationToken ct)
    {
        return ApplyAsync(id, actorUserId, (incident, context) =>
        {
            DemandRevision(incident, "title", expectedRevision, incident.TitleRevision, incident.Title);
            incident.UpdateTitle(title, context);
        }, ct);
    }

    public Task<IncidentResponse?> UpdateDescriptionAsync(Guid id, string description, int expectedRevision, int actorUserId, CancellationToken ct)
    {
        return ApplyAsync(id, actorUserId, (incident, context) =>
        {
            DemandRevision(incident, "description", expectedRevision, incident.DescriptionRevision, incident.Description);
            incident.UpdateDescription(description, context);
        }, ct);
    }

    public Task<IncidentResponse?> UpdatePriorityAsync(Guid id, IncidentPriority priority, int expectedRevision, int actorUserId, CancellationToken ct)
    {
        return ApplyAsync(id, actorUserId, (incident, context) =>
        {
            DemandRevision(incident, "priority", expectedRevision, incident.PriorityRevision, incident.Priority.ToString());
            incident.UpdatePriority(priority, context);
        }, ct);
    }

    public async Task<IncidentResponse?> UpdateAssigneeAsync(Guid id, int? assigneeUserId, int expectedRevision, int actorUserId, CancellationToken ct)
    {
        User? assignee = null;

        if (assigneeUserId is not null)
        {
            assignee = await context.Users.FirstOrDefaultAsync(
                user => user.UserId == assigneeUserId && user.IsActive && user.Role != UserRole.Pending, ct);

            if (assignee is null)
            {
                throw new ArgumentException("The assignee must be an active operator or admin.");
            }
        }

        return await ApplyAsync(id, actorUserId, (incident, context) =>
        {
            DemandRevision(incident, "assignee", expectedRevision, incident.AssigneeRevision, incident.AssigneeUserId?.ToString() ?? "");
            incident.UpdateAssignee(assignee, context);
        }, ct);
    }

    private async Task<IncidentResponse?> ApplyAsync(Guid id, int actorUserId, Action<Incident, ChangeContext> update, CancellationToken ct)
    {
        var incident = await LoadWithHistoryAsync(id, ct);

        if (incident is null)
        {
            return null;
        }

        update(incident, NewContext(actorUserId));
        await context.SaveChangesAsync(ct);

        return IncidentResponse.From(incident);
    }

    private ChangeContext NewContext(int actorUserId)
    {
        return new ChangeContext(timeProvider.GetUtcNow().UtcDateTime, actorUserId);
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
            .Include(i => i.AssigneeHistory)
            .Include(i => i.Assignee)
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
            .Select(c => new IncidentChangeResponse("created", c.OccurredAtUtc, null, null, c.ActorUserId, c.Actor != null ? c.Actor.Name : null))
            .ToListAsync(ct);

        var status = await MapKindAsync(context.IncidentStatusChanges.AsNoTracking().Where(c => c.IncidentId == incidentId), "status", ct);
        var title = await MapKindAsync(context.IncidentTitleChanges.AsNoTracking().Where(c => c.IncidentId == incidentId), "title", ct);
        var description = await MapKindAsync(context.IncidentDescriptionChanges.AsNoTracking().Where(c => c.IncidentId == incidentId), "description", ct);
        var priority = await MapKindAsync(context.IncidentPriorityChanges.AsNoTracking().Where(c => c.IncidentId == incidentId), "priority", ct);
        var assignee = await context.IncidentAssigneeChanges.AsNoTracking()
            .Where(c => c.IncidentId == incidentId)
            .Select(c => new IncidentChangeResponse(
                "assignee",
                c.OccurredAtUtc,
                context.Users.Where(user => user.UserId == c.OldValue).Select(user => user.Name).FirstOrDefault(),
                context.Users.Where(user => user.UserId == c.NewValue).Select(user => user.Name).FirstOrDefault(),
                c.ActorUserId,
                c.Actor != null ? c.Actor.Name : null))
            .ToListAsync(ct);

        return created
            .Concat(status)
            .Concat(title)
            .Concat(description)
            .Concat(priority)
            .Concat(assignee)
            .OrderBy(e => e.OccurredAtUtc)
            .ToList();
    }

    private static async Task<List<IncidentChangeResponse>> MapKindAsync<T>(
        IQueryable<IncidentChange<T>> changes, string kind, CancellationToken ct)
    {
        var rows = await changes
            .OrderBy(c => c.OccurredAtUtc)
            .ThenBy(c => c.Id)
            .Select(c => new { c.OccurredAtUtc, c.OldValue, c.NewValue, c.ActorUserId, ActorName = c.Actor != null ? c.Actor.Name : null })
            .ToListAsync(ct);

        return rows
            .Select(r => new IncidentChangeResponse(kind, r.OccurredAtUtc, r.OldValue?.ToString(), r.NewValue?.ToString(), r.ActorUserId, r.ActorName))
            .ToList();
    }
}
