
using incidenskezelo_backend.Models.AI;
using incidenskezelo_backend.Services.AI;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using incidenskezelo_backend.Auth;
using Microsoft.AspNetCore.Authorization;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize(Policy = AuthPolicies.CanHandleIncidents)]
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


        try
        {
            var result = await _incidentAnalysisService.AnalyzeAsync(
                incidentId,
                request.Model,
                cancellationToken);

            return Ok(new
            {
                incidentId,
                model = result.Model,
                summary = result.Summary,
                probableCauses = result.ProbableCauses,
                suggestedActions = result.SuggestedActions,
                relevantEventIds = result.RelevantEventIds,
                limitations = result.Limitations,
                toolCalls = result.ToolCalls
            });
        }
        catch (AiProviderTimeoutException)
        {
            return Problem(
                statusCode: StatusCodes.Status504GatewayTimeout,
                title: "AI request timed out",
                detail: "The AI model did not respond in time. Please try again.");
        }
        catch (AiProviderUnavailableException)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "AI provider unavailable",
                detail: "The AI model is currently unavailable. Please try again later.");
        }
        catch (AiInvalidResponseException)
        {
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "AI response could not be used",
                detail: "The AI model returned an invalid response. Please try again.");
        }
    }

    [HttpGet("analyze/{incidentId}")]
    public async Task<IActionResult> GetLatest(
    Guid incidentId,
    CancellationToken cancellationToken)
    {
        if (incidentId == Guid.Empty)
        {
            return BadRequest("The incident ID is required.");
        }

        var stored = await _incidentAnalysisService.GetLatestAsync(
            incidentId,
            cancellationToken);

        if (stored is null)
        {
            return NotFound();
        }

        var result = JsonSerializer.Deserialize<IncidentAnalysisResult>(
            stored.ResultJson,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        if (result is null)
        {
            // Ez sérült adatbázisban tárolt eredmény, nem provider-hiba.
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Stored analysis could not be read",
                detail: "The saved analysis could not be loaded.");
        }

        return Ok(new
        {
            incidentId,
            model = result.Model,
            analyzedAtUtc = stored.AnalyzedAtUtc,
            summary = result.Summary,
            probableCauses = result.ProbableCauses,
            suggestedActions = result.SuggestedActions,
            relevantEventIds = result.RelevantEventIds,
            limitations = result.Limitations,
            toolCalls = result.ToolCalls
        });
    }
}
