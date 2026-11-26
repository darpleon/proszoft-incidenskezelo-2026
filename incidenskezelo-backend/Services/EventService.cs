using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json;
using System;
using System.Linq;
using incidenskezelo_backend.Services.IncidentAutomation;

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
    private readonly IncidentCorrelationService _correlationService;
    private readonly AutomaticIncidentService _automaticIncidentService;
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    private readonly AppDbContext _dbContext;
    private readonly ILogger<EventService> _logger;

    public EventService(AppDbContext dbContext, ILogger<EventService> logger, IncidentCorrelationService correlationService, AutomaticIncidentService automaticIncidentService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _correlationService = correlationService;
        _automaticIncidentService = automaticIncidentService;
    }


    public async Task<EventIngestResult> IngestAsync(
    CreateEventRequest request,
    CancellationToken cancellationToken)
    {
        if (!TryParseName(request.EventType, out EventType eventType))
        {
            return new EventIngestResult(
                EventIngestStatus.Invalid,
                null,
                $"Unknown event type '{request.EventType}'.");
        }

        if (!TryParseName(request.Severity, out Severity severity))
        {
            return new EventIngestResult(
                EventIngestStatus.Invalid,
                null,
                $"Unknown severity '{request.Severity}'.");
        }

        var contentHash = CreateContentHash(request, eventType, severity);
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                // SQL retry esetén a tranzakció újrafut ugyanazzal a DbContexttel.
                // A tracker ürítése eltávolítja az előző próbálkozás állapotát.
                _dbContext.ChangeTracker.Clear();

                await using var transaction =
                    await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                var result = await IngestWithinTransactionAsync(
                    request,
                    eventType,
                    severity,
                    contentHash,
                    cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return result;
            });
        }
        catch (DbUpdateException exception) when (IsDuplicateKey(exception))
        {
            // Egy párhuzamos kézbesítés elmenthette előbb ugyanazt az eventet.
            _dbContext.ChangeTracker.Clear();

            var concurrentDuplicate =
                await FindBySourceIdAsync(contentHash, cancellationToken);

            return new EventIngestResult(
                EventIngestStatus.AlreadyReceived,
                concurrentDuplicate,
                null);
        }
    }

    private async Task<EventIngestResult> IngestWithinTransactionAsync(
    CreateEventRequest request,
    EventType eventType,
    Severity severity,
    string contentHash,
    CancellationToken cancellationToken)
    {
        var existing = await FindBySourceIdAsync(contentHash, cancellationToken);

        if (existing is not null)
        {
            _logger.LogInformation(
                "Duplicate event {ContentHash} ignored",
                contentHash);

            return new EventIngestResult(
                EventIngestStatus.AlreadyReceived,
                existing,
                null);
        }

        var service = await _dbContext.Services.SingleOrDefaultAsync(
            item => item.Code == request.ServiceCode && item.IsActive,
            cancellationToken);

        if (service is null)
        {
            return new EventIngestResult(
                EventIngestStatus.Invalid,
                null,
                $"Unknown service code '{request.ServiceCode}'.");
        }

        var newEvent = new Event
        {
            SourceEventId = contentHash,
            CorrelationKey = request.CorrelationKey,
            ServiceId = service.ServiceId,
            EventType = eventType,
            Severity = severity,
            OccurredAtUtc = request.OccurredAtUtc!.Value.ToUniversalTime(),
            ReceivedAtUtc = DateTime.UtcNow,
            Summary = request.Summary,
            PayloadJson = request.Payload?.GetRawText(),
        };

        _dbContext.Events.Add(newEvent);

        // Külön mentés kell, hogy az event backend által kiosztott EventId-ja
        // rendelkezésre álljon az IncidentEventLink auditbejegyzéshez.
        await _dbContext.SaveChangesAsync(cancellationToken);

        var classification = IncidentCorrelationRules.Classify(eventType);
        var candidate = await _correlationService.FindCandidateAsync(
            newEvent,
            cancellationToken);

        Guid? linkedIncidentPublicId = null;

        if (candidate.Status == IncidentCandidateStatus.Unique)
        {
            var incident = candidate.Incident!;

            _automaticIncidentService.AttachEvent(incident, newEvent);
            linkedIncidentPublicId = incident.PublicId;

            _logger.LogInformation(
                "Event {EventId} linked to incident {IncidentId} by {MatchRule}",
                newEvent.EventId,
                incident.PublicId,
                candidate.MatchRule);
        }
        else if (candidate.Status == IncidentCandidateStatus.Ambiguous)
        {
            _logger.LogWarning(
                "Event {EventId} matched multiple incidents by {MatchRule}; left unlinked",
                newEvent.EventId,
                candidate.MatchRule);
        }
        else if (classification.Role == IncidentEventRole.Trigger &&
                 classification.Family is { } family)
        {
            var incident = _automaticIncidentService.CreateForEvent(
                newEvent,
                service,
                family);

            linkedIncidentPublicId = incident.PublicId;

            _logger.LogInformation(
                "Incident {IncidentId} created automatically for event {EventId}",
                incident.PublicId,
                newEvent.EventId);
        }
        else
        {
            _logger.LogInformation(
                "Event {EventId} stored without an incident",
                newEvent.EventId);
        }

        // Elmenti az incidentet, az eventkapcsolatot és az esetleges prioritásváltozást.
        // Státuszváltás nincs: automatikus lezárás nem történik.
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new EventIngestResult(
            EventIngestStatus.Created,
            ToResponse(newEvent, service.Code, linkedIncidentPublicId),
            null);
    }

    //hash számítás a tartalom alapján, hogy a duplikációt ellenőrizni lehessen
    private static string CreateContentHash(
    CreateEventRequest request,
    EventType eventType,
    Severity severity)
    {
        var occurredAtUtc = request.OccurredAtUtc!.Value.ToUniversalTime();

        using var buffer = new MemoryStream();

        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("serviceCode", request.ServiceCode);
            writer.WriteString("eventType", eventType.ToString());
            writer.WriteString("severity", severity.ToString());
            writer.WriteString("occurredAtUtc", occurredAtUtc.ToString("O"));
            writer.WriteString("correlationKey", request.CorrelationKey?.Trim().ToLowerInvariant());

            if (request.Summary is null)
                writer.WriteNull("summary");
            else
                writer.WriteString("summary", request.Summary);


            //payload-nál nem számít a sorrend
            writer.WritePropertyName("payload");
            
            if (request.Payload is { } payload)
                WriteCanonicalJson(payload, writer);
            else
                writer.WriteNullValue();

            writer.WriteEndObject();
        }

        var hashBytes = SHA256.HashData(buffer.ToArray());
        return Convert.ToHexString(hashBytes);
    }

    //segédmetódus a canonical json írásához a payload-ból, hogy a sorrend ne számítson a hash számításnál
    private static void WriteCanonicalJson(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();

                foreach (var property in element.EnumerateObject()
                             .OrderBy(property => property.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonicalJson(property.Value, writer);
                }

                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();

                foreach (var item in element.EnumerateArray())
                    WriteCanonicalJson(item, writer);

                writer.WriteEndArray();
                break;

            default:
                element.WriteTo(writer);
                break;
        }
    }

    public async Task<List<EventResponse>> GetLatestAsync(int count, CancellationToken cancellationToken)
    {
        return await ToResponses(_dbContext.Events
            .OrderByDescending(evt => evt.EventId)
            .Take(count))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EventResponse>> GetRecentByServiceCodeAsync(
    string serviceCode,
    int count,
    CancellationToken cancellationToken)
    {
        return await ToResponses(_dbContext.Events
            .Where(evt => evt.Service.Code == serviceCode)
            .OrderByDescending(evt => evt.OccurredAtUtc)
            .ThenByDescending(evt => evt.EventId)
            .Take(count))
            .ToListAsync(cancellationToken);
    }


    public async Task<List<EventResponse>?> GetByIncidentAsync(Guid incidentId, CancellationToken cancellationToken)
    {
        var internalId = await _dbContext.Incidents
            .Where(incident => incident.PublicId == incidentId)
            .Select(incident => (int?)incident.IncidentId)
            .FirstOrDefaultAsync(cancellationToken);

        if (internalId is null)
        {
            return null;
        }

        return await ToResponses(_dbContext.Events
            .Where(evt => evt.IncidentId == internalId)
            .OrderBy(evt => evt.OccurredAtUtc))
            .ToListAsync(cancellationToken);
    }


    public async Task<EventResponse?> GetByIdAsync(long eventId, CancellationToken cancellationToken)
    {
        return await ToResponses(_dbContext.Events.Where(evt => evt.EventId == eventId))
            .SingleOrDefaultAsync(cancellationToken);
    }


    private static bool TryParseName<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
    {
        return Enum.TryParse(value, ignoreCase: true, out result)
            && Enum.GetNames<TEnum>().Contains(value, StringComparer.OrdinalIgnoreCase);
    }


    private async Task<EventResponse?> FindBySourceIdAsync(string sourceEventId, CancellationToken cancellationToken)
    {
        return await ToResponses(_dbContext.Events.Where(evt => evt.SourceEventId == sourceEventId))
            .SingleOrDefaultAsync(cancellationToken);
    }


    private static bool IsDuplicateKey(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && sqlException.Number is UniqueIndexViolation or UniqueConstraintViolation;
    }


    private IQueryable<EventResponse> ToResponses(IQueryable<Event> events)
    {
        return events.Select(evt => ToResponse(
            evt,
            evt.Service.Code,
            _dbContext.Incidents
                .Where(incident => incident.IncidentId == evt.IncidentId)
                .Select(incident => (Guid?)incident.PublicId)
                .FirstOrDefault()));
    }


    private static EventResponse ToResponse(Event evt, string serviceCode, Guid? incidentId)
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
            CorrelationKey = evt.CorrelationKey,
            IncidentId = incidentId,
        };
    }
}
