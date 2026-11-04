using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private const int LatestEventCount = 100;

    private readonly EventService _eventService;
    private readonly EventIngestService _ingestService;
    private readonly EventIngestOptions _ingestOptions;

    public EventsController(
        EventService eventService,
        EventIngestService ingestService,
        IOptions<EventIngestOptions> ingestOptions)
    {
        _eventService = eventService;
        _ingestService = ingestService;
        _ingestOptions = ingestOptions.Value;
    }


    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        if (!_ingestOptions.DirectHttpEnabled)
        {
            return NotFound();
        }

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
