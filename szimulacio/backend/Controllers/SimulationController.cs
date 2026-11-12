using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimulatorController : ControllerBase
{
    private readonly SimulatorService _simulator;
    private readonly IEventPublisher _publisher;

    private readonly ScenarioCatalog _scenarioCatalog;

    private readonly ScenarioRunner _scenarioRunner;

    public SimulatorController(
    SimulatorService simulator,
    IEventPublisher publisher,
    ScenarioCatalog scenarioCatalog,
    ScenarioRunner scenarioRunner)
    {
        _simulator = simulator;
        _publisher = publisher;
        _scenarioCatalog = scenarioCatalog;
        _scenarioRunner = scenarioRunner;
    }


    [HttpPost("generate")]
    public async Task<IActionResult> GenerateEvent(CancellationToken cancellationToken)
    {
        var simulatedEvent = _simulator.GenerateEvent();

        var result = await _publisher.PublishAsync(simulatedEvent, cancellationToken);

        if (!result.IsPublished)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { simulatedEvent, published = false, error = result.Error });
        }

        return Ok(new { simulatedEvent, published = true });
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
