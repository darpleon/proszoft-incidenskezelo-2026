
using incidenskezelo_backend.Services.AI;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeminiController : ControllerBase
{
    private readonly IIncidentAnalysisService _incidentAnalysisService;

    public GeminiController(IIncidentAnalysisService incidentAnalysisService)
    {
        _incidentAnalysisService = incidentAnalysisService;
    }

    //API endpoint az incidens elemzéséhez, itt ID alapján elemez
    [HttpPost("analyze/{incidentId}")]
    public async Task<IActionResult> Analyze(
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        if (incidentId == Guid.Empty)
        {
            return BadRequest("Az incidens azonosítója kötelező.");
        }

        var result = await _incidentAnalysisService.AnalyzeAsync(
            incidentId,
            cancellationToken);

        return Ok(new
        {
            incidentId,
            analysis = result
        });
    }
}
