using backend.Contracts;
using backend.Controllers;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
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

    // A teszt ellenőrzi, hogy a forgatókönyv lépésének mezői és Payload értéke
    // változatlanul átkerülnek a publikált eseménybe.
    [TestMethod]
    public async Task TryStart_ScenarioStep_PublishesAllStepValues()
    {
        var publisher = new RecordingEventPublisher();
        var runner = CreateRunner(publisher);
        var payload = new { orderId = 123, attempt = 2 };
        var scenario = CreateScenario(new ScenarioStep
        {
            DelaySeconds = 0,
            CorrelationKey = "correlation-123",
            ServiceCode = "orders-api",
            EventType = EventType.HighLatency,
            Severity = Severity.Warning,
            Summary = "High response time",
            Payload = payload
        });

        Assert.IsTrue(runner.TryStart(scenario));

        var publishedEvent = await publisher.WaitForEventCountAsync(1);

        Assert.AreEqual("correlation-123", publishedEvent[0].CorrelationKey);
        Assert.AreEqual("orders-api", publishedEvent[0].ServiceCode);
        Assert.AreEqual(EventType.HighLatency, publishedEvent[0].EventType);
        Assert.AreEqual(Severity.Warning, publishedEvent[0].Severity);
        Assert.AreEqual("High response time", publishedEvent[0].Summary);
        Assert.AreSame(payload, publishedEvent[0].Payload);
    }

    // A teszt ellenőrzi, hogy null időeltolás esetén az esemény időpontja
    // a forgatókönyv futtatásakor aktuális UTC-idő lesz.
    [TestMethod]
    public async Task TryStart_NullOccurredAtOffset_UsesCurrentUtcTime()
    {
        var publisher = new RecordingEventPublisher();
        var runner = CreateRunner(publisher);
        var scenario = CreateScenario(new ScenarioStep
        {
            DelaySeconds = 0,
            OccurredAtOffsetSeconds = null,
            ServiceCode = "payment-gw",
            EventType = EventType.HighLatency,
            Severity = Severity.Warning
        });

        var beforeStart = DateTime.UtcNow;

        Assert.IsTrue(runner.TryStart(scenario));

        var publishedEvent = await publisher.WaitForEventCountAsync(1);
        var afterPublish = DateTime.UtcNow;
        var occurredAtUtc = publishedEvent[0].OccurredAtUtc;

        Assert.AreEqual(DateTimeKind.Utc, occurredAtUtc.Kind);
        Assert.IsTrue(
            occurredAtUtc >= beforeStart && occurredAtUtc <= afterPublish,
            $"Az esemény időpontja ({occurredAtUtc:O}) nem esik a teszt futása alatti időintervallumba.");
    }

    // A teszt ellenőrzi, hogy a -20 másodperces időeltolás
    // húsz másodperccel korábbi UTC-időt eredményez.
    [TestMethod]
    public async Task TryStart_NegativeOccurredAtOffset_AppliesOffsetToCurrentUtcTime()
    {
        var publisher = new RecordingEventPublisher();
        var runner = CreateRunner(publisher);
        var scenario = CreateScenario(new ScenarioStep
        {
            DelaySeconds = 0,
            OccurredAtOffsetSeconds = -20,
            ServiceCode = "payment-gw",
            EventType = EventType.HighLatency,
            Severity = Severity.Warning
        });

        var beforeStart = DateTime.UtcNow;

        Assert.IsTrue(runner.TryStart(scenario));

        var publishedEvent = await publisher.WaitForEventCountAsync(1);
        var afterPublish = DateTime.UtcNow;
        var occurredAtUtc = publishedEvent[0].OccurredAtUtc;

        Assert.AreEqual(DateTimeKind.Utc, occurredAtUtc.Kind);
        Assert.IsTrue(
            occurredAtUtc >= beforeStart.AddSeconds(-20) &&
            occurredAtUtc <= afterPublish.AddSeconds(-20),
            $"Az esemény időpontja ({occurredAtUtc:O}) nem felel meg a -20 másodperces eltolásnak.");
    }

    // A teszt ellenőrzi, hogy több forgatókönyv-lépés eseményei
    // a megadott lépéssorrendben jutnak el a publikálóhoz.
    [TestMethod]
    public async Task TryStart_MultipleSteps_PublishesEventsInStepOrder()
    {
        var publisher = new RecordingEventPublisher();
        var runner = CreateRunner(publisher);
        var scenario = CreateScenario(
            new ScenarioStep
            {
                DelaySeconds = 0,
                ServiceCode = "first-service",
                EventType = EventType.HighLatency,
                Severity = Severity.Warning,
                CorrelationKey = "first"
            },
            new ScenarioStep
            {
                DelaySeconds = 0,
                ServiceCode = "second-service",
                EventType = EventType.ServiceDown,
                Severity = Severity.Critical,
                CorrelationKey = "second"
            });

        Assert.IsTrue(runner.TryStart(scenario));

        var publishedEvents = await publisher.WaitForEventCountAsync(2);

        Assert.HasCount(2, publishedEvents);
        Assert.AreEqual("first-service", publishedEvents[0].ServiceCode);
        Assert.AreEqual("first", publishedEvents[0].CorrelationKey);
        Assert.AreEqual("second-service", publishedEvents[1].ServiceCode);
        Assert.AreEqual("second", publishedEvents[1].CorrelationKey);
    }

    // A teszt ellenőrzi, hogy egy forgatókönyv befejezése után
    // ugyanaz a runner új forgatókönyv indítását is elfogadja.
    [TestMethod]
    public async Task TryStart_AfterPreviousScenarioFinished_AllowsAnotherStart()
    {
        var publisher = new RecordingEventPublisher();
        var runner = CreateRunner(publisher);
        var scenario = CreateScenario(new ScenarioStep
        {
            DelaySeconds = 0,
            ServiceCode = "payment-gw",
            EventType = EventType.HighLatency,
            Severity = Severity.Warning
        });

        Assert.IsTrue(runner.TryStart(scenario));
        await publisher.WaitForEventCountAsync(1);

        // A publikálás befejeződése után a runner finally ága visszaállítja
        // a futási állapotot. Rövid ideig újrapróbálkozunk, amíg ez megtörténik.
        var restarted = await TryStartWhenAvailableAsync(runner, scenario);

        Assert.IsTrue(restarted, "A befejezett forgatókönyv után új indításnak sikerülnie kell.");

        var publishedEvents = await publisher.WaitForEventCountAsync(2);

        Assert.HasCount(2, publishedEvents);
    }

    // A teszt ellenőrzi, hogy sikertelen publikálási eredmény után
    // a runner a következő forgatókönyv-lépést is megpróbálja publikálni.
    [TestMethod]
    public async Task TryStart_PublishReturnsFailure_ContinuesWithNextStep()
    {
        var publisher = new RecordingEventPublisher(
            _ => new EventPublishResult(EventPublishStatus.Failed, "Test publish failure"));
        var runner = CreateRunner(publisher);
        var scenario = CreateScenario(
            new ScenarioStep
            {
                DelaySeconds = 0,
                ServiceCode = "first-service",
                EventType = EventType.HighLatency,
                Severity = Severity.Warning
            },
            new ScenarioStep
            {
                DelaySeconds = 0,
                ServiceCode = "second-service",
                EventType = EventType.ServiceDown,
                Severity = Severity.Critical
            });

        Assert.IsTrue(runner.TryStart(scenario));

        var publishedEvents = await publisher.WaitForEventCountAsync(2);

        Assert.HasCount(2, publishedEvents);
        Assert.AreEqual("first-service", publishedEvents[0].ServiceCode);
        Assert.AreEqual("second-service", publishedEvents[1].ServiceCode);
    }

    // A teszt ellenőrzi, hogy a publikáló kivétele leállítja az aktuális futást,
    // de a runner futási állapota felszabadul, így később újraindítható.
    [TestMethod]
    public async Task TryStart_PublisherThrows_StopsRunAndAllowsRestart()
    {
        var publisher = new ThrowOnceEventPublisher();
        var runner = CreateRunner(publisher);
        var scenario = CreateScenario(
            new ScenarioStep
            {
                DelaySeconds = 0,
                ServiceCode = "first-service",
                EventType = EventType.HighLatency,
                Severity = Severity.Warning
            },
            new ScenarioStep
            {
                DelaySeconds = 0,
                ServiceCode = "second-service",
                EventType = EventType.ServiceDown,
                Severity = Severity.Critical
            });

        Assert.IsTrue(runner.TryStart(scenario));

        await publisher.WaitForCallCountAsync(1);

        // Az első hívás kivétellel végződik. Megvárjuk, amíg a runner
        // finally ága felszabadítja a futási állapotot.
        var restarted = await TryStartWhenAvailableAsync(runner, scenario);

        Assert.IsTrue(restarted, "A kivétel után a runnernek újraindíthatónak kell lennie.");

        await publisher.WaitForCallCountAsync(2);

        Assert.AreEqual(2, publisher.CallCount);
    }

    private static ScenarioRunner CreateRunner(IEventPublisher publisher)
    {
        return new ScenarioRunner(
            publisher,
            NullLogger<ScenarioRunner>.Instance);
    }

    private static Scenario CreateScenario(params ScenarioStep[] steps)
    {
        return new Scenario
        {
            Id = "test-scenario",
            Name = "Test scenario",
            Steps = steps.ToList()
        };
    }

    private static async Task<bool> TryStartWhenAvailableAsync(
        ScenarioRunner runner,
        Scenario scenario)
    {
        var timeout = TimeSpan.FromSeconds(5);
        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < timeout)
        {
            if (runner.TryStart(scenario))
            {
                return true;
            }

            await Task.Delay(10);
        }

        return false;
    }

    private sealed class RecordingEventPublisher : IEventPublisher
    {
        private readonly ConcurrentQueue<SimulationEvent> _events = new();
        private readonly SemaphoreSlim _eventPublished = new(0);
        private readonly Func<int, EventPublishResult> _resultFactory;

        public RecordingEventPublisher(
            Func<int, EventPublishResult>? resultFactory = null)
        {
            _resultFactory = resultFactory ??
                (_ => new EventPublishResult(EventPublishStatus.Published));
        }
        public Task<EventPublishResult> PublishAsync(
            SimulationEvent simulatedEvent,
            CancellationToken cancellationToken)
        {
            _events.Enqueue(simulatedEvent);
            _eventPublished.Release();

            return Task.FromResult(_resultFactory(_events.Count));
        }

        public async Task<IReadOnlyList<SimulationEvent>> WaitForEventCountAsync(
            int expectedCount)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            while (_events.Count < expectedCount)
            {
                await _eventPublished.WaitAsync(timeout.Token);
            }

            return _events.ToArray();
        }
    }

    private sealed class ThrowOnceEventPublisher : IEventPublisher
    {
        private readonly SemaphoreSlim _callMade = new(0);
        private int _callCount;

        public int CallCount => Volatile.Read(ref _callCount);

        public Task<EventPublishResult> PublishAsync(
            SimulationEvent simulatedEvent,
            CancellationToken cancellationToken)
        {
            var callNumber = Interlocked.Increment(ref _callCount);
            _callMade.Release();

            if (callNumber == 1)
            {
                throw new InvalidOperationException("Test publisher exception.");
            }

            return Task.FromResult(
                new EventPublishResult(EventPublishStatus.Published));
        }

        public async Task WaitForCallCountAsync(int expectedCount)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            while (CallCount < expectedCount)
            {
                await _callMade.WaitAsync(timeout.Token);
            }
        }
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

    // A controller tesztjei a generált esemény publikálásának HTTP-válaszait ellenőrzik.
    [TestClass]
    public class SimulatorControllerTests
    {
        // A teszt ellenőrzi, hogy sikeres publikálás esetén a controller
        // 200 OK választ ad vissza.
        [TestMethod]
        public async Task GenerateEvent_PublishSucceeds_ReturnsOk()
        {
            var publisher = new ControllerTestPublisher(
                new EventPublishResult(EventPublishStatus.Published));
            var controller = CreateController(publisher);

            var result = await controller.GenerateEvent(CancellationToken.None);

            Assert.IsInstanceOfType<OkObjectResult>(result);
            Assert.AreEqual(
                StatusCodes.Status200OK,
                ((OkObjectResult)result).StatusCode);
        }

        // A teszt ellenőrzi, hogy publikálási hiba esetén a controller
        // 502 Bad Gateway választ ad vissza.
        [TestMethod]
        public async Task GenerateEvent_PublishFails_ReturnsBadGateway()
        {
            var publisher = new ControllerTestPublisher(
                new EventPublishResult(EventPublishStatus.Failed, "Broker unavailable"));
            var controller = CreateController(publisher);

            var result = await controller.GenerateEvent(CancellationToken.None);

            Assert.IsInstanceOfType<ObjectResult>(result);
            Assert.AreEqual(
                StatusCodes.Status502BadGateway,
                ((ObjectResult)result).StatusCode);
        }

        private static SimulatorController CreateController(IEventPublisher publisher)
        {
            // A GenerateEvent metódus nem használja a katalógust és a forgatókönyv-runnert,
            // ezért ezek ebben a két tesztben nem szükségesek.
            return new SimulatorController(
                new SimulatorService(),
                publisher,
                null!,
                null!);
        }

        private sealed class ControllerTestPublisher : IEventPublisher
        {
            private readonly EventPublishResult _result;

            public ControllerTestPublisher(EventPublishResult result)
            {
                _result = result;
            }

            public Task<EventPublishResult> PublishAsync(
                SimulationEvent simulatedEvent,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(_result);
            }
        }
    }
}
