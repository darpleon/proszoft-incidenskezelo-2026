
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeminiController : ControllerBase
{
    private readonly IAiService _aiService;

    public GeminiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    //API endpoint az incidens elemzéséhez, itt ID alapján elemez
    [HttpPost("analyze/{incidentId}")]
    public async Task<IActionResult> Analyze(
        string incidentId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
        {
            return BadRequest("Az incidens azonosítója kötelező.");
        }

        var prompt = $"""
            {IncidentAnalysisPrompt.SystemPrompt}

            Az elemzendő incidens azonosítója:
            {incidentId}

            Jelenleg még nem kaptál adatbázisból származó
            incidensadatokat. Ne következtess az incidens
            tartalmára pusztán az azonosítóból.
            """;

        var result = await _aiService.GenerateAsync(
            prompt,
            cancellationToken);

        return Ok(new
        {
            incidentId,
            analysis = result
        });
    }
}
