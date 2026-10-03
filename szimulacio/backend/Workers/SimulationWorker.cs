using backend.Models;
using backend.Services;

namespace Simulator.Workers;

public class SimulationWorker : BackgroundService
{
    private readonly SimulatorService _simulator;
    private readonly IncidentApiClient _incidentApi;
    private readonly ILogger<SimulationWorker> _logger;

    public SimulationWorker(
        SimulatorService simulator,
        IncidentApiClient incidentApi,
        ILogger<SimulationWorker> logger)
    {
        _simulator = simulator;
        _incidentApi = incidentApi;
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

            await SendAsync(simulatedEvent, stoppingToken);

            await Task.Delay(
                TimeSpan.FromSeconds(10),
                stoppingToken);
        }
    }

    private async Task SendAsync(SimulationEvent simulatedEvent, CancellationToken stoppingToken)
    {
        try
        {
            using var response = await _incidentApi.SendEventAsync(simulatedEvent, stoppingToken);

            _logger.LogInformation(
                "Sent event {EventId}: {StatusCode}",
                simulatedEvent.EventId,
                (int)response.StatusCode);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                "Could not send event {EventId}: {Message}",
                simulatedEvent.EventId,
                exception.Message);
        }
    }
}
