using incidenskezelo_backend.Models.Incident;
using incidenskezelo_backend.Models.Incident.Changes;
using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/incidents")]
public class IncidentsController(IIncidentService incidentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<IncidentResponse>>> GetAll(CancellationToken ct)
    {
        return await incidentService.GetAllAsync(ct);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IncidentResponse>> GetById(int id, CancellationToken ct)
    {
        var incident = await incidentService.GetByIdAsync(id, ct);
        return incident is null ? NotFound() : incident;
    }

    [HttpPost]
    public async Task<ActionResult<IncidentResponse>> Create(CreateIncidentRequest request, CancellationToken ct)
    {
        var errors = new List<string>();
        Collect(errors, IncidentValidator.TitleError(request.Title));
        Collect(errors, IncidentValidator.DescriptionError(request.Description));

        if (errors.Count > 0)
        {
            return Invalid(errors);
        }

        var incident = await incidentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = incident.IncidentId }, incident);
    }

    [HttpPatch("{id:int}/title")]
    public async Task<ActionResult<IncidentResponse>> UpdateTitle(int id, UpdateTitleRequest request, CancellationToken ct)
    {
        if (IncidentValidator.TitleError(request.Title) is { } error)
        {
            return Invalid([error]);
        }

        var incident = await incidentService.UpdateTitleAsync(id, request.Title, ct);
        return incident is null ? NotFound() : incident;
    }

    [HttpPatch("{id:int}/description")]
    public async Task<ActionResult<IncidentResponse>> UpdateDescription(int id, UpdateDescriptionRequest request, CancellationToken ct)
    {
        if (IncidentValidator.DescriptionError(request.Description) is { } error)
        {
            return Invalid([error]);
        }

        var incident = await incidentService.UpdateDescriptionAsync(id, request.Description, ct);
        return incident is null ? NotFound() : incident;
    }

    [HttpPatch("{id:int}/priority")]
    public async Task<ActionResult<IncidentResponse>> UpdatePriority(int id, UpdatePriorityRequest request, CancellationToken ct)
    {
        var incident = await incidentService.UpdatePriorityAsync(id, request.Priority, ct);
        return incident is null ? NotFound() : incident;
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

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<IncidentResponse>> Transition(int id, TransitionRequest request, CancellationToken ct)
    {
        try
        {
            var incident = await incidentService.TransitionAsync(id, request.Status, ct);
            return incident is null ? NotFound() : incident;
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

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<List<IncidentStatusChangeResponse>>> GetHistory(int id, CancellationToken ct)
    {
        var history = await incidentService.GetHistoryAsync(id, ct);
        return history is null ? NotFound() : history;
    }
}
