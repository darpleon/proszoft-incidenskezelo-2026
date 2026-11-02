using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using incidenskezelo_backend.Models.Incident;
using incidenskezelo_backend.Models.Incident.Changes;
using incidenskezelo_backend.Services;
using incidenskezelo_backend.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace incidenskezelo_backend.Tests.Services;

[TestClass]
public class EventServiceTests
{
    private AppDbContext _dbContext = null!;
    private EventService _eventService = null!;


    [TestInitialize]
    public void Setup()
    {
        _dbContext = TestDbContextFactory.Create();
        _eventService = new EventService(_dbContext, NullLogger<EventService>.Instance);
    }


    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }


    [TestMethod]
    public async Task IngestAsync_ValidEvent_StoresEvent()
    {
        var request = CreateRequest(serviceCode: "orders-api");

        var result = await _eventService.IngestAsync(request, CancellationToken.None);

        Assert.AreEqual(EventIngestStatus.Created, result.Status);
        Assert.AreEqual(1, _dbContext.Events.Count());

        var stored = _dbContext.Events.Single();
        Assert.AreEqual(64, stored.SourceEventId.Length);
        Assert.AreEqual(EventType.HighLatency, stored.EventType);
        Assert.AreEqual(Severity.Warning, stored.Severity);
        Assert.AreNotEqual(default, stored.ReceivedAtUtc);
    }


    [TestMethod]
    public async Task IngestAsync_SameContentTwice_StoresOnlyOneEvent()
    {
        var first = await _eventService.IngestAsync(CreateRequest(), CancellationToken.None);

        var second = await _eventService.IngestAsync(CreateRequest(), CancellationToken.None);

        Assert.AreEqual(EventIngestStatus.Created, first.Status);
        Assert.AreEqual(EventIngestStatus.AlreadyReceived, second.Status);
        Assert.AreEqual(first.Event!.EventId, second.Event!.EventId);
        Assert.AreEqual(1, _dbContext.Events.Count());
    }


    [TestMethod]
    public async Task IngestAsync_UnknownServiceCode_ReturnsInvalid()
    {
        var request = CreateRequest(serviceCode: "unknown-service");

        var result = await _eventService.IngestAsync(request, CancellationToken.None);

        Assert.AreEqual(EventIngestStatus.Invalid, result.Status);
        Assert.AreEqual("Unknown service code 'unknown-service'.", result.Error);
        Assert.AreEqual(0, _dbContext.Events.Count());
    }


    [TestMethod]
    [DataRow("Timeout", "Warning", DisplayName = "unknown event type")]
    [DataRow("HighLatency", "Urgent", DisplayName = "unknown severity")]
    [DataRow("HighLatency", "3", DisplayName = "severity as a number")]
    public async Task IngestAsync_UnknownEventTypeOrSeverity_ReturnsInvalid(string eventType, string severity)
    {
        var request = CreateRequest(eventType: eventType, severity: severity);

        var result = await _eventService.IngestAsync(request, CancellationToken.None);

        Assert.AreEqual(EventIngestStatus.Invalid, result.Status);
        Assert.AreEqual(0, _dbContext.Events.Count());
    }


    [TestMethod]
    public async Task GetByIncidentAsync_LinkedEvents_ReturnsOnlyThoseEvents()
    {
        await _eventService.IngestAsync(CreateRequest(summary: "linked"), CancellationToken.None);
        await _eventService.IngestAsync(CreateRequest(summary: "other"), CancellationToken.None);
        var incident = Incident.Create("Orders API errors", "POST /orders returns 500.", IncidentPriority.High, new ChangeContext(DateTime.UtcNow, null));
        incident.Events.Add(_dbContext.Events.Single(evt => evt.Summary == "linked"));
        _dbContext.Incidents.Add(incident);
        await _dbContext.SaveChangesAsync();

        var events = await _eventService.GetByIncidentAsync(incident.PublicId, CancellationToken.None);

        Assert.AreEqual(1, events!.Count);
        Assert.AreEqual("linked", events[0].Summary);
        Assert.AreEqual(incident.PublicId, events[0].IncidentId);
    }


    [TestMethod]
    public async Task GetByIncidentAsync_UnknownIncident_ReturnsNull()
    {
        var events = await _eventService.GetByIncidentAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsNull(events);
    }


    [TestMethod]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var result = await _eventService.GetByIdAsync(999, CancellationToken.None);

        Assert.IsNull(result);
    }


    private static CreateEventRequest CreateRequest(
        string serviceCode = "orders-api",
        string eventType = "HighLatency",
        string severity = "Warning",
        string summary = "Response time above threshold")
    {
        return new CreateEventRequest
        {
            ServiceCode = serviceCode,
            EventType = eventType,
            Severity = severity,
            OccurredAtUtc = new DateTime(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc),
            Summary = summary,
        };
    }
}
