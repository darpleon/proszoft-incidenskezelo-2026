using incidenskezelo_backend.Models.Incident;
using incidenskezelo_backend.Models.Incident.Changes;

namespace incidenskezelo_backend.Services;

public interface IIncidentService
{
    Task<List<IncidentResponse>> GetAllAsync(IncidentQuery query, CancellationToken ct);
    Task<IncidentResponse?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IncidentResponse> CreateAsync(CreateIncidentRequest request, int actorUserId, CancellationToken ct);
    Task<IncidentResponse?> TransitionAsync(Guid id, IncidentStatus status, int expectedRevision, int actorUserId, CancellationToken ct);
    Task<IncidentResponse?> UpdateTitleAsync(Guid id, string title, int expectedRevision, int actorUserId, CancellationToken ct);
    Task<IncidentResponse?> UpdateDescriptionAsync(Guid id, string description, int expectedRevision, int actorUserId, CancellationToken ct);
    Task<IncidentResponse?> UpdatePriorityAsync(Guid id, IncidentPriority priority, int expectedRevision, int actorUserId, CancellationToken ct);
    Task<IncidentResponse?> UpdateAssigneeAsync(Guid id, int? assigneeUserId, int expectedRevision, int actorUserId, CancellationToken ct);
    Task<IncidentCommentResponse?> AddCommentAsync(Guid id, string body, int actorUserId, CancellationToken ct);
    Task<List<IncidentCommentResponse>?> GetCommentsAsync(Guid id, CancellationToken ct);
    Task<IncidentCommentResponse?> GetCommentAsync(Guid id, int commentIndex, CancellationToken ct);
    Task<IncidentCommentResponse?> UpdateCommentBodyAsync(Guid id, int commentIndex, string body, int expectedRevision, int actorUserId, CancellationToken ct);
    Task<List<IncidentChangeResponse>?> GetHistoryAsync(Guid id, CancellationToken ct);
    Task<List<PreviousIncidentSearchResult>> SearchPreviousAsync(Guid excludeIncidentId, string query, int limit, CancellationToken ct);
}
