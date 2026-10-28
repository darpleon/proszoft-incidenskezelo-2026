using System.Net;
using backend.Configuration;
using backend.Models;
using backend.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace szimulacio.Tests.Services;

[TestClass]
public class ScenarioRunnerTests
{
    [TestMethod]
    public async Task TryStart_ScenarioAlreadyRunning_ReturnsFalse()
    {
        var handler = new BlockingHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new IncidentApiOptions
        {
            BaseUrl = "http://localhost"
        });
        var incidentApiClient = new IncidentApiClient(httpClient, options);
        var scenarioRunner = new ScenarioRunner(
            incidentApiClient,
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

            var requestBody = await handler.RequestBody.WaitAsync(TimeSpan.FromSeconds(5));

            using var document = JsonDocument.Parse(requestBody);
            var root = document.RootElement;

            Assert.AreEqual("payment-gw", root.GetProperty("serviceCode").GetString());
            Assert.AreEqual("HighLatency", root.GetProperty("eventType").GetString());
            Assert.AreEqual("Warning", root.GetProperty("severity").GetString());
            Assert.AreEqual("Test scenario event", root.GetProperty("summary").GetString());
            Assert.AreEqual("test-correlation", root.GetProperty("correlationKey").GetString());

            Assert.IsFalse(scenarioRunner.TryStart(scenario));

            Assert.IsFalse(scenarioRunner.TryStart(scenario));
        }
        finally
        {
            handler.ReleaseRequest();
            httpClient.Dispose();
        }
    }

    private sealed class BlockingHttpMessageHandler : HttpMessageHandler
    {
        private readonly TaskCompletionSource<string> _requestBody =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly TaskCompletionSource _releaseRequest =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<string> RequestBody => _requestBody.Task;

        public void ReleaseRequest()
        {
            _releaseRequest.TrySetResult();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            _requestBody.TrySetResult(body);

            await _releaseRequest.Task.WaitAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}