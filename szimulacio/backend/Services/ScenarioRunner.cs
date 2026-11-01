using backend.Models;

namespace backend.Services;

public class ScenarioRunner
{
    private readonly IEventPublisher _publisher;
    private readonly ILogger<ScenarioRunner> _logger;
    private readonly object _lock = new();

    private bool _isRunning;

    public ScenarioRunner(
        IEventPublisher publisher,
        ILogger<ScenarioRunner> logger)
    {
        _publisher = publisher;
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
                    OccurredAtUtc = step.OccurredAtUtc ?? DateTime.UtcNow,
                    Summary = step.Summary,
                    Payload = step.Payload
                };

                var result = await _publisher.PublishAsync(
                    simulatedEvent,
                    CancellationToken.None);

                if (result.IsPublished)
                {
                    _logger.LogInformation(
                        "Event from scenario {ScenarioId} published.",
                        scenario.Id);
                }
                else
                {
                    _logger.LogWarning(
                        "Could not publish event from scenario {ScenarioId}: {Error}",
                        scenario.Id,
                        result.Error);
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