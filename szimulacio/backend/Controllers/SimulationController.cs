using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimulatorController : ControllerBase
{
    private readonly SimulatorService _simulator;

    public SimulatorController(SimulatorService simulator)
    {
        _simulator = simulator;
    }

    [HttpGet("status")]
    public ActionResult<SimulatorStatus> GetStatus()
    {
        var status = _simulator.GetStatus();

        return Ok(status);
    }

    [HttpPost("generate")]
    public ActionResult<SimulationEvent> GenerateEvent()
    {
        var simulatedEvent = _simulator.GenerateEvent();

        return Ok(simulatedEvent);
    }
}