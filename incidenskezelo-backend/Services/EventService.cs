using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace incidenskezelo_backend.Services;

public enum EventIngestStatus
{
    Created,
    AlreadyReceived,
    Invalid,
}

public record EventIngestResult(EventIngestStatus Status, EventResponse? Event, string? Error);

public class EventService
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    private readonly AppDbContext _dbContext;
    private readonly ILogger<EventService> _logger;

    public EventService(AppDbContext dbContext, ILogger<EventService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }


    public async Task<EventIngestResult> IngestAsync(CreateEventRequest request, CancellationToken cancellationToken)
    {
        if (!TryParseName(request.EventType, out EventType eventType))
        {
            return new EventIngestResult(EventIngestStatus.Invalid, null, $"Unknown event type '{request.EventType}'.");
        }

        if (!TryParseName(request.Severity, out Severity severity))
        {
            return new EventIngestResult(EventIngestStatus.Invalid, null, $"Unknown severity '{request.Severity}'.");
        }

        var existing = await FindBySourceIdAsync(request.SourceEventId, cancellationToken);
        if (existing is not null)
        {
            _logger.LogInformation("Duplicate event {SourceEventId} ignored", request.SourceEventId);
            return new EventIngestResult(EventIngestStatus.AlreadyReceived, existing, null);
        }

        var service = await _dbContext.Services.SingleOrDefaultAsync(
            item => item.Code == request.ServiceCode && item.IsActive,
            cancellationToken);
        if (service is null)
        {
            return new EventIngestResult(EventIngestStatus.Invalid, null, $"Unknown service code '{request.ServiceCode}'.");
        }

        var newEvent = new Event
        {
            SourceEventId = request.SourceEventId,
            ServiceId = service.ServiceId,
            EventType = eventType,
            Severity = severity,
            OccurredAtUtc = request.OccurredAtUtc!.Value.ToUniversalTime(),
            ReceivedAtUtc = DateTime.UtcNow,
            Summary = request.Summary,
            PayloadJson = request.Payload?.GetRawText(),
        };

        _dbContext.Events.Add(newEvent);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateKey(exception))
        {
            _dbContext.Entry(newEvent).State = EntityState.Detached;
            var concurrentDuplicate = await FindBySourceIdAsync(request.SourceEventId, cancellationToken);
            return new EventIngestResult(EventIngestStatus.AlreadyReceived, concurrentDuplicate, null);
        }

        _logger.LogInformation(
            "Event {EventId} stored ({SourceEventId}, {ServiceCode}, {EventType})",
            newEvent.EventId, newEvent.SourceEventId, service.Code, newEvent.EventType);

        return new EventIngestResult(EventIngestStatus.Created, ToResponse(newEvent, service.Code), null);
    }


    public async Task<List<EventResponse>> GetLatestAsync(int count, CancellationToken cancellationToken)
    {
        return await _dbContext.Events
            .OrderByDescending(evt => evt.EventId)
            .Take(count)
            .Select(evt => ToResponse(evt, evt.Service.Code))
            .ToListAsync(cancellationToken);
    }


    public async Task<EventResponse?> GetByIdAsync(long eventId, CancellationToken cancellationToken)
    {
        return await _dbContext.Events
            .Where(evt => evt.EventId == eventId)
            .Select(evt => ToResponse(evt, evt.Service.Code))
            .SingleOrDefaultAsync(cancellationToken);
    }


    private static bool TryParseName<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
    {
        return Enum.TryParse(value, ignoreCase: true, out result)
            && Enum.GetNames<TEnum>().Contains(value, StringComparer.OrdinalIgnoreCase);
    }


    private async Task<EventResponse?> FindBySourceIdAsync(string sourceEventId, CancellationToken cancellationToken)
    {
        return await _dbContext.Events
            .Where(evt => evt.SourceEventId == sourceEventId)
            .Select(evt => ToResponse(evt, evt.Service.Code))
            .SingleOrDefaultAsync(cancellationToken);
    }


    private static bool IsDuplicateKey(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && sqlException.Number is UniqueIndexViolation or UniqueConstraintViolation;
    }


    private static EventResponse ToResponse(Event evt, string serviceCode)
    {
        return new EventResponse
        {
            EventId = evt.EventId,
            SourceEventId = evt.SourceEventId,
            ServiceCode = serviceCode,
            EventType = evt.EventType,
            Severity = evt.Severity,
            OccurredAtUtc = evt.OccurredAtUtc,
            ReceivedAtUtc = evt.ReceivedAtUtc,
            Summary = evt.Summary,
            PayloadJson = evt.PayloadJson,
        };
    }
}
