using backend.Contracts;
using backend.Models;
using backend.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace szimulacio.Tests.Services;

[TestClass]
public class ScenarioRunnerTests
{
    [TestMethod]
    public async Task TryStart_ScenarioAlreadyRunning_ReturnsFalse()
    {
        var publisher = new BlockingEventPublisher();
        var scenarioRunner = new ScenarioRunner(
            publisher,
            NullLogger<ScenarioRunner>.Instance);
        var scenario = new Scenario
        {
            Id = "test-scenario",
            Name = "Test scenario",
            Steps =
            [
                new ScenarioStep
                {
                    DelaySeconds = 0,
                    ServiceCode = "payment-gw",
                    EventType = EventType.HighLatency,
                    Severity = Severity.Warning,
                    CorrelationKey = "test-correlation",
                    Summary = "Test scenario event"
                }
            ]
        };

        try
        {
            Assert.IsTrue(scenarioRunner.TryStart(scenario));

            var publishedEvent = await publisher.PublishedEvent.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.AreEqual("payment-gw", publishedEvent.ServiceCode);
            Assert.AreEqual(EventType.HighLatency, publishedEvent.EventType);
            Assert.AreEqual(Severity.Warning, publishedEvent.Severity);
            Assert.AreEqual("Test scenario event", publishedEvent.Summary);
            Assert.AreEqual("test-correlation", publishedEvent.CorrelationKey);

            Assert.IsFalse(scenarioRunner.TryStart(scenario));

            Assert.IsFalse(scenarioRunner.TryStart(scenario));
        }
        finally
        {
            publisher.Release();
        }
    }

    [TestMethod]
    public void CreateEventRequest_SerializesToPinnedWireShape()
    {
        var request = new CreateEventRequest
        {
            ServiceCode = "payment-gw",
            EventType = "HighLatency",
            Severity = "Warning",
            OccurredAtUtc = new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc),
            Summary = "Test scenario event",
            CorrelationKey = "test-correlation",
        };

        using var document = JsonDocument.Parse(
            JsonSerializer.SerializeToUtf8Bytes(
                request,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        var root = document.RootElement;

        CollectionAssert.AreEqual(
            new[] { "serviceCode", "eventType", "severity", "occurredAtUtc", "summary", "payload", "correlationKey" },
            root.EnumerateObject().Select(property => property.Name).ToArray());

        Assert.AreEqual("payment-gw", root.GetProperty("serviceCode").GetString());
        Assert.AreEqual("HighLatency", root.GetProperty("eventType").GetString());
        Assert.AreEqual("Warning", root.GetProperty("severity").GetString());
        Assert.AreEqual("Test scenario event", root.GetProperty("summary").GetString());
        Assert.AreEqual("test-correlation", root.GetProperty("correlationKey").GetString());
    }

    private sealed class BlockingEventPublisher : IEventPublisher
    {
        private readonly TaskCompletionSource<SimulationEvent> _publishedEvent =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly TaskCompletionSource _release =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<SimulationEvent> PublishedEvent => _publishedEvent.Task;

        public void Release()
        {
            _release.TrySetResult();
        }

        public async Task<EventPublishResult> PublishAsync(
            SimulationEvent simulatedEvent,
            CancellationToken cancellationToken)
        {
            _publishedEvent.TrySetResult(simulatedEvent);

            await _release.Task.WaitAsync(cancellationToken);

            return new EventPublishResult(EventPublishStatus.Published);
        }
    }
}
