using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace incidenskezelo_backend.Services;

public enum EventIngestOutcomeStatus
{
    Malformed,
    Invalid,
    Created,
    AlreadyReceived,
    TransientFailed,
}

public record EventIngestOutcome(EventIngestOutcomeStatus Status, EventResponse? Event, string? Error);

public class EventIngestService
{
    private const string TransientFailureError =
        "The event could not be stored; a transient failure persisted after retries.";

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EventIngestOptions _options;
    private readonly ILogger<EventIngestService> _logger;

    public EventIngestService(
        IServiceScopeFactory scopeFactory,
        IOptions<EventIngestOptions> options,
        ILogger<EventIngestService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EventIngestOutcome> IngestAsync(
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken)
    {
        CreateEventRequest? request;

        try
        {
            request = JsonSerializer.Deserialize<CreateEventRequest>(body.Span, JsonOptions);
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Event payload is not readable JSON; rejecting it.");
            return new EventIngestOutcome(EventIngestOutcomeStatus.Malformed, null, "The event payload is not readable JSON.");
        }

        if (request is null)
        {
            return new EventIngestOutcome(EventIngestOutcomeStatus.Malformed, null, "The event payload is not readable JSON.");
        }

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(
                request,
                new ValidationContext(request),
                validationResults,
                validateAllProperties: true))
        {
            var error = string.Join("; ", validationResults.Select(result => result.ErrorMessage));
            _logger.LogWarning("Rejected invalid event: {Error}", error);
            return new EventIngestOutcome(EventIngestOutcomeStatus.Invalid, null, error);
        }

        for (var attempt = 1; attempt <= _options.RetryAttempts; attempt++)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

                var result = await eventService.IngestAsync(request, cancellationToken);

                if (result.Status == EventIngestStatus.Invalid)
                {
                    _logger.LogWarning("Rejected invalid event: {Error}", result.Error);
                }

                return result.Status switch
                {
                    EventIngestStatus.Created => new EventIngestOutcome(EventIngestOutcomeStatus.Created, result.Event, null),
                    EventIngestStatus.AlreadyReceived => new EventIngestOutcome(EventIngestOutcomeStatus.AlreadyReceived, result.Event, null),
                    _ => new EventIngestOutcome(EventIngestOutcomeStatus.Invalid, null, result.Error),
                };
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                if (attempt == _options.RetryAttempts)
                {
                    _logger.LogError(
                        exception,
                        "Event ingest failed after {Attempts} attempts; giving up.",
                        attempt);

                    return new EventIngestOutcome(EventIngestOutcomeStatus.TransientFailed, null, TransientFailureError);
                }

                _logger.LogWarning(
                    exception,
                    "Event ingest failed (attempt {Attempt}/{Attempts}); retrying in {Delay}.",
                    attempt,
                    _options.RetryAttempts,
                    TimeSpan.FromSeconds(1 << attempt));

                await Task.Delay(TimeSpan.FromSeconds(1 << attempt), cancellationToken);
            }
        }

        throw new InvalidOperationException("Unreachable: the retry loop always returns.");
    }
}
