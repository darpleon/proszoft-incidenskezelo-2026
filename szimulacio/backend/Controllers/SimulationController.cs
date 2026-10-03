using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimulatorController : ControllerBase
{
    private readonly SimulatorService _simulator;
    private readonly IncidentApiClient _incidentApi;

    public SimulatorController(SimulatorService simulator, IncidentApiClient incidentApi)
    {
        _simulator = simulator;
        _incidentApi = incidentApi;
    }

    [HttpGet("status")]
    public ActionResult<SimulatorStatus> GetStatus()
    {
        var status = _simulator.GetStatus();

        return Ok(status);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateEvent(CancellationToken cancellationToken)
    {
        var simulatedEvent = _simulator.GenerateEvent();

        try
        {
            using var response = await _incidentApi.SendEventAsync(simulatedEvent, cancellationToken);

            return Ok(new { simulatedEvent, deliveryStatusCode = (int)response.StatusCode });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { simulatedEvent, error = "Incident API is not reachable." });
        }
    }
}