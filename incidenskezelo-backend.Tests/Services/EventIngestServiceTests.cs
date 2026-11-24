using System.Text;
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using incidenskezelo_backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace incidenskezelo_backend.Tests.Services;

[TestClass]
public class EventIngestServiceTests
{
    private ServiceProvider _serviceProvider = null!;
    private EventIngestService _ingestService = null!;


    [TestInitialize]
    public void Setup()
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddScoped<EventService>();
        services.AddScoped<RejectedEventService>();
        services.AddSingleton(TimeProvider.System);

        _serviceProvider = services.BuildServiceProvider();

        using (var scope = _serviceProvider.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        }

        _ingestService = new EventIngestService(
            _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new EventIngestOptions { RetryAttempts = 1 }),
            NullLogger<EventIngestService>.Instance);
    }


    [TestCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }


    [TestMethod]
    public async Task IngestAsync_BrokenJson_RecordsRejectedEvent()
    {
        const string body = """{"serviceCode":"orders-api","eventType":""";

        var outcome = await _ingestService.IngestAsync(Encoding.UTF8.GetBytes(body), RejectedEventSource.Queue, CancellationToken.None);

        Assert.AreEqual(EventIngestOutcomeStatus.Malformed, outcome.Status);
        Assert.IsTrue(outcome.Recorded);

        var rejected = ReadRejectedEvents().Single();
        Assert.AreEqual(RejectedEventSource.Queue, rejected.Source);
        Assert.AreEqual(RejectedEventReason.Malformed, rejected.Reason);
        Assert.AreEqual(body, rejected.RawPayload);
        Assert.AreEqual("The event payload is not readable JSON.", rejected.Error);
    }


    [TestMethod]
    public async Task IngestAsync_UnknownServiceCode_RecordsRejectedEvent()
    {
        const string body = """
            {"serviceCode":"orders-apii","eventType":"HighLatency","severity":"Warning","occurredAtUtc":"2026-11-17T12:00:00Z"}
            """;

        var outcome = await _ingestService.IngestAsync(Encoding.UTF8.GetBytes(body), RejectedEventSource.Http, CancellationToken.None);

        Assert.AreEqual(EventIngestOutcomeStatus.Invalid, outcome.Status);
        Assert.IsTrue(outcome.Recorded);

        var rejected = ReadRejectedEvents().Single();
        Assert.AreEqual(RejectedEventSource.Http, rejected.Source);
        Assert.AreEqual(RejectedEventReason.Invalid, rejected.Reason);
        Assert.AreEqual("Unknown service code 'orders-apii'.", rejected.Error);
    }


    [TestMethod]
    public async Task IngestAsync_ValidEvent_DoesNotRecordRejectedEvent()
    {
        const string body = """
            {"serviceCode":"orders-api","eventType":"HighLatency","severity":"Warning","occurredAtUtc":"2026-11-17T12:00:00Z"}
            """;

        var outcome = await _ingestService.IngestAsync(Encoding.UTF8.GetBytes(body), RejectedEventSource.Queue, CancellationToken.None);

        Assert.AreEqual(EventIngestOutcomeStatus.Created, outcome.Status);
        Assert.IsFalse(outcome.Recorded);
        Assert.AreEqual(0, ReadRejectedEvents().Count);
    }


    private List<RejectedEvent> ReadRejectedEvents()
    {
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>().RejectedEvents.ToList();
    }
}
