using backend.Models;
using backend.Services;

namespace Simulator.Workers;

public class SimulationWorker : BackgroundService
{
    private readonly SimulatorService _simulator;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<SimulationWorker> _logger;

    public SimulationWorker(
        SimulatorService simulator,
        IEventPublisher publisher,
        ILogger<SimulationWorker> logger)
    {
        _simulator = simulator;
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Simulation worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var simulatedEvent = _simulator.GenerateEvent();

            _logger.LogInformation("Generated event");

            await SendAsync(simulatedEvent, stoppingToken);

            await Task.Delay(
                TimeSpan.FromSeconds(10),
                stoppingToken);
        }
    }

    private async Task SendAsync(SimulationEvent simulatedEvent, CancellationToken stoppingToken)
    {
        var result = await _publisher.PublishAsync(simulatedEvent, stoppingToken);

        if (result.IsPublished)
        {
            _logger.LogInformation("Published event.");
        }
        else
        {
            _logger.LogWarning(
                "Could not publish event: {Error}",
                result.Error);
        }
    }
}
