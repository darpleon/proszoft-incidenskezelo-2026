using incidenskezelo_backend.Auth;
using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/rejected-events")]
[Authorize(Policy = AuthPolicies.CanHandleIncidents)]
public class RejectedEventsController : ControllerBase
{
    private const int LatestRejectedEventCount = 100;

    private readonly RejectedEventService _rejectedEventService;

    public RejectedEventsController(RejectedEventService rejectedEventService)
    {
        _rejectedEventService = rejectedEventService;
    }


    [HttpGet]
    public async Task<IActionResult> GetLatest(CancellationToken cancellationToken)
    {
        var rejectedEvents = await _rejectedEventService.GetLatestAsync(LatestRejectedEventCount, cancellationToken);

        return Ok(rejectedEvents);
    }
}
