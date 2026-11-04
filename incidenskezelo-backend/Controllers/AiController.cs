
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
}
