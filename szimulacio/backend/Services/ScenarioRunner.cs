using backend.Models;

namespace backend.Services;

public class ScenarioRunner
{
    private readonly IncidentApiClient _incidentApi;
    private readonly ILogger<ScenarioRunner> _logger;
    private readonly object _lock = new();

    private bool _isRunning;

    public ScenarioRunner(
        IncidentApiClient incidentApi,
        ILogger<ScenarioRunner> logger)
    {
        _incidentApi = incidentApi;
        _logger = logger;
    }

    public bool TryStart(Scenario scenario)
    {
        lock (_lock)
        {
            if (_isRunning)
            {
                return false;
            }

            _isRunning = true;
        }

        _ = Task.Run(() => RunAsync(scenario));

        return true;
    }

    private async Task RunAsync(Scenario scenario)
    {
        _logger.LogInformation(
            "Scenario {ScenarioId} started.",
            scenario.Id);

        try
        {
            foreach (var step in scenario.Steps)
            {
                await Task.Delay(TimeSpan.FromSeconds(step.DelaySeconds));

                var simulatedEvent = new SimulationEvent
                {
                    CorrelationKey = step.CorrelationKey,
                    ServiceCode = step.ServiceCode,
                    EventType = step.EventType,
                    Severity = step.Severity,
                    OccurredAtUtc = DateTime.UtcNow,
                    Summary = step.Summary,
                    Payload = step.Payload
                };

                try
                {
                    using var response = await _incidentApi.SendEventAsync(
                        simulatedEvent,
                        CancellationToken.None);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning(
                            "Event from scenario {ScenarioId} returned HTTP {StatusCode}.",
                            scenario.Id,
                            (int)response.StatusCode);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Event from scenario {ScenarioId} sent with HTTP {StatusCode}.",
                            scenario.Id,
                            (int)response.StatusCode);
                    }
                }
                catch (HttpRequestException exception)
                {
                    _logger.LogError(
                        exception,
                        "Could not send an event from scenario {ScenarioId}.",
                        scenario.Id);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Scenario {ScenarioId} failed.",
                scenario.Id);
        }
        finally
        {
            lock (_lock)
            {
                _isRunning = false;
            }

            _logger.LogInformation(
                "Scenario {ScenarioId} finished.",
                scenario.Id);
        }
    }
}