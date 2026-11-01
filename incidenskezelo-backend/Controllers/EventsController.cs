using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private const int LatestEventCount = 100;

    private readonly EventService _eventService;
    private readonly EventIngestService _ingestService;

    public EventsController(EventService eventService, EventIngestService ingestService)
    {
        _eventService = eventService;
        _ingestService = ingestService;
    }


    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        using var body = new MemoryStream();
        await Request.Body.CopyToAsync(body, cancellationToken);

        var outcome = await _ingestService.IngestAsync(body.ToArray(), cancellationToken);

        return outcome.Status switch
        {
            EventIngestOutcomeStatus.Created => CreatedAtAction(
                nameof(GetById), new { eventId = outcome.Event!.EventId }, outcome.Event),
            EventIngestOutcomeStatus.AlreadyReceived => Ok(outcome.Event),
            EventIngestOutcomeStatus.TransientFailed => Problem(
                outcome.Error, statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => Problem(outcome.Error, statusCode: StatusCodes.Status400BadRequest),
        };
    }


    [HttpGet]
    public async Task<IActionResult> GetLatest(CancellationToken cancellationToken)
    {
        var events = await _eventService.GetLatestAsync(LatestEventCount, cancellationToken);

        return Ok(events);
    }


    [HttpGet("{eventId:long}")]
    public async Task<IActionResult> GetById(long eventId, CancellationToken cancellationToken)
    {
        var evt = await _eventService.GetByIdAsync(eventId, cancellationToken);

        return evt is null ? NotFound() : Ok(evt);
    }
}
