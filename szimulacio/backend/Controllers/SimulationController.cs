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

    private readonly ScenarioCatalog _scenarioCatalog;

    private readonly ScenarioRunner _scenarioRunner;

    public SimulatorController(
    SimulatorService simulator,
    IncidentApiClient incidentApi,
    ScenarioCatalog scenarioCatalog,
    ScenarioRunner scenarioRunner)
    {
        _simulator = simulator;
        _incidentApi = incidentApi;
        _scenarioCatalog = scenarioCatalog;
        _scenarioRunner = scenarioRunner;
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

    [HttpGet("scenarios")]
    public ActionResult<IReadOnlyCollection<Scenario>> GetScenarios()
    {
        return Ok(_scenarioCatalog.GetAll());
    }

    [HttpPost("scenarios/{scenarioId}/start")]
    public IActionResult StartScenario(string scenarioId)
    {
        if (!_scenarioCatalog.TryGet(scenarioId, out var scenario) ||
            scenario is null)
        {
            return NotFound(new
            {
                error = $"Scenario '{scenarioId}' was not found."
            });
        }

        if (!_scenarioRunner.TryStart(scenario))
        {
            return Conflict(new
            {
                error = "A scenario is already running."
            });
        }

        return Accepted(new
        {
            scenarioId = scenario.Id,
            message = "Scenario started."
        });
    }
}