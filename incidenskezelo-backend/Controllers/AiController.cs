
using incidenskezelo_backend.Models.AI;
using incidenskezelo_backend.Services.AI;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IIncidentAnalysisService _incidentAnalysisService;

    public AiController(IIncidentAnalysisService incidentAnalysisService)
    {
        _incidentAnalysisService = incidentAnalysisService;
    }

    //API endpoint az incidens elemzéséhez, itt ID alapján elemez
    [HttpPost("analyze/{incidentId}")]
    public async Task<IActionResult> Analyze(
        Guid incidentId,
        [FromBody] AnalyzeIncidentRequest request,
        CancellationToken cancellationToken)
    {
        if (incidentId == Guid.Empty)
        {
            return BadRequest("The incident ID is required.");
        }

        var result = await _incidentAnalysisService.AnalyzeAsync(
            incidentId,
            request.Model,
            cancellationToken);

        return Ok(new
        {
            incidentId,
            analysis = result
        });
    }
}
