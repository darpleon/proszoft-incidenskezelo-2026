using backend.Services;

namespace Simulator.Workers;

public class SimulationWorker : BackgroundService
{
    private readonly SimulatorService _simulator;
    private readonly ILogger<SimulationWorker> _logger;

    public SimulationWorker(
        SimulatorService simulator,
        ILogger<SimulationWorker> logger)
    {
        _simulator = simulator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Simulation worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var simulatedEvent = _simulator.GenerateEvent();

            _logger.LogInformation(
                "Generated event: {EventId}",
                simulatedEvent.EventId);

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }
}