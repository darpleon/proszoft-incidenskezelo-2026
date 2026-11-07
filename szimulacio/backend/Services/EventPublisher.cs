using System.Text.Json;
using backend.Configuration;
using backend.Contracts;
using backend.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace backend.Services;

public sealed class EventPublisher : IEventPublisher
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOptions _options;
    private readonly ILogger<EventPublisher> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private IConnection? _connection;
    private IChannel? _channel;

    public EventPublisher(IOptions<RabbitMqOptions> options, ILogger<EventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EventPublishResult> PublishAsync(
        SimulationEvent simulatedEvent,
        CancellationToken cancellationToken)
    {
        var request = CreateEventRequest.FromSimulationEvent(simulatedEvent);
        var body = JsonSerializer.SerializeToUtf8Bytes(request, JsonOptions);

        try
        {
            var channel = await GetChannelAsync(cancellationToken);

            await channel.BasicPublishAsync(
                exchange: _options.Exchange,
                routingKey: _options.RoutingKey,
                mandatory: true,
                basicProperties: new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent,
                },
                body: body,
                cancellationToken: cancellationToken);

            return new EventPublishResult(EventPublishStatus.Published);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Publish to {Exchange}/{RoutingKey} failed.",
                _options.Exchange,
                _options.RoutingKey);

            await DisposeChannelAsync();
            return new EventPublishResult(EventPublishStatus.Failed, exception.Message);
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            await DisposeChannelAsync();

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                AutomaticRecoveryEnabled = true,
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true),
                cancellationToken);

            return _channel;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task DisposeChannelAsync()
    {
        if (_channel is { } channel)
        {
            _channel = null;
            try
            {
                await channel.CloseAsync();
                await channel.DisposeAsync();
            }
            catch
            {
            }
        }

        if (_connection is { } connection)
        {
            _connection = null;
            try
            {
                await connection.CloseAsync();
                await connection.DisposeAsync();
            }
            catch
            {
            }
        }
    }
}
