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
        var invalid = ValidateCreate(request);
        if (invalid is not null)
        {
            return invalid;
        }

        var incident = await incidentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = incident.IncidentId }, incident);
    }

    private ActionResult? ValidateCreate(CreateIncidentRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add("Title is required.");
        }
        else if (request.Title.Length > IncidentLimits.TitleMaxLength)
        {
            errors.Add($"Title must be at most {IncidentLimits.TitleMaxLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            errors.Add("Description is required.");
        }
        else if (request.Description.Length > IncidentLimits.DescriptionMaxLength)
        {
            errors.Add($"Description must be at most {IncidentLimits.DescriptionMaxLength} characters.");
        }

        if (errors.Count == 0)
        {
            return null;
        }

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
