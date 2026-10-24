using incidenskezelo_backend.Auth;
using incidenskezelo_backend.Models.Incident;
using incidenskezelo_backend.Models.Incident.Changes;
using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/incidents")]
[Authorize(Policy = AuthPolicies.CanHandleIncidents)]
public class IncidentsController(IIncidentService incidentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<IncidentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IncidentResponse>>> GetAll(CancellationToken ct)
    {
        return await incidentService.GetAllAsync(ct);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentResponse>> GetById(Guid id, CancellationToken ct)
    {
        var incident = await incidentService.GetByIdAsync(id, ct);
        return incident is null ? Missing(id) : incident;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentResponse>> Create(CreateIncidentRequest request, CancellationToken ct)
    {
        var errors = new List<string>();
        Collect(errors, IncidentValidator.TitleError(request.Title));
        Collect(errors, IncidentValidator.DescriptionError(request.Description));

        if (errors.Count > 0)
        {
            return Invalid(errors);
        }

        var incident = await incidentService.CreateAsync(request, CurrentUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = incident.Id }, incident);
    }

    [HttpPatch("{id}/title")]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IncidentResponse>> UpdateTitle(Guid id, UpdateTitleRequest request, CancellationToken ct)
    {
        if (IncidentValidator.TitleError(request.Title) is { } error)
        {
            return Invalid([error]);
        }

        if (request.ExpectedRevision is not { } expectedRevision)
        {
            return Invalid(["expectedRevision is required."]);
        }

        try
        {
            var incident = await incidentService.UpdateTitleAsync(id, request.Title, expectedRevision, CurrentUserId(), ct);
            return incident is null ? Missing(id) : incident;
        }
        catch (RevisionConflictException ex)
        {
            return RevisionConflict(ex);
        }
    }

    [HttpPatch("{id}/description")]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IncidentResponse>> UpdateDescription(Guid id, UpdateDescriptionRequest request, CancellationToken ct)
    {
        if (IncidentValidator.DescriptionError(request.Description) is { } error)
        {
            return Invalid([error]);
        }

        if (request.ExpectedRevision is not { } expectedRevision)
        {
            return Invalid(["expectedRevision is required."]);
        }

        try
        {
            var incident = await incidentService.UpdateDescriptionAsync(id, request.Description, expectedRevision, CurrentUserId(), ct);
            return incident is null ? Missing(id) : incident;
        }
        catch (RevisionConflictException ex)
        {
            return RevisionConflict(ex);
        }
    }

    [HttpPatch("{id}/priority")]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IncidentResponse>> UpdatePriority(Guid id, UpdatePriorityRequest request, CancellationToken ct)
    {
        if (request.ExpectedRevision is not { } expectedRevision)
        {
            return Invalid(["expectedRevision is required."]);
        }

        try
        {
            var incident = await incidentService.UpdatePriorityAsync(id, request.Priority, expectedRevision, CurrentUserId(), ct);
            return incident is null ? Missing(id) : incident;
        }
        catch (RevisionConflictException ex)
        {
            return RevisionConflict(ex);
        }
    }

    [HttpPatch("{id}/assignee")]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IncidentResponse>> UpdateAssignee(Guid id, UpdateAssigneeRequest request, CancellationToken ct)
    {
        if (request.ExpectedRevision is not { } expectedRevision)
        {
            return Invalid(["expectedRevision is required."]);
        }

        try
        {
            var incident = await incidentService.UpdateAssigneeAsync(id, request.AssigneeUserId, expectedRevision, CurrentUserId(), ct);
            return incident is null ? Missing(id) : incident;
        }
        catch (ArgumentException ex)
        {
            return Invalid([ex.Message]);
        }
        catch (RevisionConflictException ex)
        {
            return RevisionConflict(ex);
        }
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IncidentResponse>> Transition(Guid id, TransitionRequest request, CancellationToken ct)
    {
        if (request.ExpectedRevision is not { } expectedRevision)
        {
            return Invalid(["expectedRevision is required."]);
        }

        try
        {
            var incident = await incidentService.TransitionAsync(id, request.Status, expectedRevision, CurrentUserId(), ct);
            return incident is null ? Missing(id) : incident;
        }
        catch (RevisionConflictException ex)
        {
            return RevisionConflict(ex);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Invalid status transition",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(List<IncidentChangeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<IncidentChangeResponse>>> GetHistory(Guid id, CancellationToken ct)
    {
        var history = await incidentService.GetHistoryAsync(id, ct);
        return history is null ? Missing(id) : history;
    }

    private int CurrentUserId()
    {
        return int.Parse(User.FindFirst(TokenClaims.UserId)!.Value);
    }

    private static void Collect(List<string> errors, string? error)
    {
        if (error is not null)
        {
            errors.Add(error);
        }
    }

    private ActionResult Invalid(List<string> errors)
    {
        return BadRequest(new ProblemDetails
        {
            Title = "Invalid incident",
            Detail = string.Join(" ", errors),
            Status = StatusCodes.Status400BadRequest
        });
    }

    private NotFoundObjectResult Missing(Guid id)
    {
        return NotFound(new ProblemDetails
        {
            Title = "Not found",
            Detail = $"No incident with id {id}.",
            Status = StatusCodes.Status404NotFound
        });
    }

    private ConflictObjectResult RevisionConflict(RevisionConflictException ex)
    {
        return Conflict(new ProblemDetails
        {
            Title = "Revision conflict",
            Detail = ex.Message,
            Status = StatusCodes.Status409Conflict,
            Extensions =
            {
                ["field"] = ex.Field,
                ["currentRevision"] = ex.CurrentRevision,
                ["currentValue"] = ex.CurrentValue,
                ["revisions"] = ex.Current
            }
        });
    }
}
