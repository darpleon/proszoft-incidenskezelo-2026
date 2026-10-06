using incidenskezelo_backend.Models;
using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/incidents")]
public class IncidentsController(IIncidentService incidentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Incident>>> GetAll(CancellationToken ct)
    {
        return await incidentService.GetAllAsync(ct);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Incident>> GetById(int id, CancellationToken ct)
    {
        var incident = await incidentService.GetByIdAsync(id, ct);
        return incident is null ? NotFound() : incident;
    }

    [HttpPost]
    public async Task<ActionResult<Incident>> Create(CreateIncidentRequest request, CancellationToken ct)
    {
        var incident = await incidentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = incident.IncidentId }, incident);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<Incident>> Transition(int id, TransitionRequest request, CancellationToken ct)
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
}
