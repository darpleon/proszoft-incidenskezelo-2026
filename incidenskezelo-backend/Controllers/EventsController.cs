using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private const int LatestEventCount = 100;

    private readonly EventService _eventService;

    public EventsController(EventService eventService)
    {
        _eventService = eventService;
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _eventService.IngestAsync(request, cancellationToken);

        return result.Status switch
        {
            EventIngestStatus.Created => CreatedAtAction(nameof(GetById), new { eventId = result.Event!.EventId }, result.Event),
            EventIngestStatus.AlreadyReceived => Ok(result.Event),
            _ => Problem(result.Error, statusCode: StatusCodes.Status400BadRequest),
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
