using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace incidenskezelo_backend.Workers;

public sealed class RabbitMqConsumer : BackgroundService
{
    private const string DeadLetterExchangeName = "events-dlx";
    private const string DeadLetterQueueName = "incident-events-dlq";

    private readonly RabbitMqOptions _options;
    private readonly EventIngestService _ingestService;
    private readonly ILogger<RabbitMqConsumer> _logger;

    private CancellationTokenSource? _stopping;

    public RabbitMqConsumer(
        IOptions<RabbitMqOptions> options,
        EventIngestService ingestService,
        ILogger<RabbitMqConsumer> logger)
    {
        _options = options.Value;
        _ingestService = ingestService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stopping = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(_stopping.Token);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(
                    exception,
                    "Broker connection lost; retrying in 5 seconds.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            AutomaticRecoveryEnabled = true,
        };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: false,
                publisherConfirmationTrackingEnabled: false),
            stoppingToken);

        await DeclareTopologyAsync(channel, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, eventArgs) =>
            HandleAsync(channel, eventArgs, stoppingToken);

        await channel.BasicConsumeAsync(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Consuming {Queue} from {HostName}.",
            _options.QueueName,
            _options.HostName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: DeadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: DeadLetterQueueName,
            exchange: DeadLetterExchangeName,
            routingKey: _options.QueueName,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = DeadLetterExchangeName,
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.QueueName,
            exchange: _options.Exchange,
            routingKey: _options.QueueName,
            cancellationToken: cancellationToken);
    }

    private async Task HandleAsync(
        IChannel channel,
        BasicDeliverEventArgs eventArgs,
        CancellationToken stoppingToken)
    {
        var outcome = await _ingestService.IngestAsync(eventArgs.Body, stoppingToken);

        if (outcome.Status is EventIngestOutcomeStatus.Malformed
            or EventIngestOutcomeStatus.TransientFailed)
        {
            await channel.BasicRejectAsync(eventArgs.DeliveryTag, requeue: false);
            return;
        }

        await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
    }

    public override void Dispose()
    {
        _stopping?.Cancel();
        _stopping?.Dispose();
        base.Dispose();
    }
}
