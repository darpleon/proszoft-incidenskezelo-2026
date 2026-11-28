using System.Text;
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using incidenskezelo_backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace incidenskezelo_backend.Tests.Services;

[TestClass]
public class EventIngestServiceTests
{
    private ServiceProvider _serviceProvider = null!;
    private EventIngestService _ingestService = null!;
    private EventSaveChangesInterceptor _saveChangesInterceptor = null!; // ÚJ: ez az interceptor szabályozottan hibát szimulál az esemény mentésekor.

    [TestInitialize]
    public void Setup()
    {
        var databaseName = Guid.NewGuid().ToString();
        _saveChangesInterceptor = new EventSaveChangesInterceptor(); // ÚJ: tesztenként külön interceptor, amely alaphelyzetben nem hibáztat.
        var services = new ServiceCollection();

        services.AddLogging();
        // MÓDOSÍTVA: services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));
        // Az interceptor csak akkor avatkozik be,amikor új Event entitást próbálunk menteni.
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName).AddInterceptors(_saveChangesInterceptor));

        services.AddScoped<EventService>();
        services.AddScoped<RejectedEventService>();
        services.AddSingleton(TimeProvider.System);

        _serviceProvider = services.BuildServiceProvider();

        using (var scope = _serviceProvider.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        }
        _ingestService = CreateIngestService(retryAttempts: 1);
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


    // ÚJ TESZT: az első mentési próbálkozás átmenetileg elbukik, a retry sikerül, és végül pontosan egy esemény kerül mentésre.
    [TestMethod]
    public async Task IngestAsync_TransientFailureThenSuccess_RetriesAndStoresEvent()
    {
        const string body = """
        {"serviceCode":"orders-api","eventType":"HighLatency","severity":"Warning","occurredAtUtc":"2026-11-17T12:00:00Z"}
        """;

        _saveChangesInterceptor.FailuresRemaining = 1;
        _ingestService = CreateIngestService(retryAttempts: 2);

        var outcome = await _ingestService.IngestAsync(
            Encoding.UTF8.GetBytes(body),
            RejectedEventSource.Queue,
            CancellationToken.None);

        Assert.AreEqual(EventIngestOutcomeStatus.Created, outcome.Status);
        Assert.IsFalse(outcome.Recorded);
        Assert.AreEqual(2, _saveChangesInterceptor.EventSaveAttempts);
        Assert.AreEqual(1, ReadEvents().Count);
        Assert.AreEqual(0, ReadRejectedEvents().Count);
    }

    // ÚJ TESZT: minden mentési próbálkozás átmeneti hibát ad, ezért az ingest a konfigurált próbálkozásszám után TransientFailed
    // eredménnyel tér vissza, normál vagy elutasított esemény mentése nélkül.
    [TestMethod]
    public async Task IngestAsync_TransientFailureOnEveryAttempt_ReturnsTransientFailed()
    {
        const string body = """
        {"serviceCode":"orders-api","eventType":"HighLatency","severity":"Warning","occurredAtUtc":"2026-11-17T12:00:00Z"}
        """;

        _saveChangesInterceptor.FailuresRemaining = 2;
        _ingestService = CreateIngestService(retryAttempts: 2);

        var outcome = await _ingestService.IngestAsync(
            Encoding.UTF8.GetBytes(body),
            RejectedEventSource.Queue,
            CancellationToken.None);

        Assert.AreEqual(EventIngestOutcomeStatus.TransientFailed, outcome.Status);
        Assert.AreEqual(
            "The event could not be stored; a transient failure persisted after retries.",
            outcome.Error);
        Assert.IsFalse(outcome.Recorded);
        Assert.AreEqual(2, _saveChangesInterceptor.EventSaveAttempts);
        Assert.AreEqual(0, ReadEvents().Count);
        Assert.AreEqual(0, ReadRejectedEvents().Count);
    }

    // ÚJ SEGÉD: adott retry-beállítással létrehozza az ingest service-t.
    private EventIngestService CreateIngestService(int retryAttempts)
    {
        return new EventIngestService(
            _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new EventIngestOptions
            {
                RetryAttempts = retryAttempts
            }),
            NullLogger<EventIngestService>.Instance);
    }

    // ÚJ SEGÉD: a külön scope-ban mentett események ellenőrzéséhez.
    private List<Event> ReadEvents()
    {
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>().Events.ToList();
    }

    // ÚJ TESZT-INFRASTRUKTÚRA: eseménymentésnél kiváltja a beállított számú átmeneti DbUpdateException hibát.
    private sealed class EventSaveChangesInterceptor : SaveChangesInterceptor
    {
        public int FailuresRemaining { get; set; }

        public int EventSaveAttempts { get; private set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var isAddingEvent = eventData.Context?.ChangeTracker
                .Entries<Event>()
                .Any(entry => entry.State == EntityState.Added) == true;

            if (isAddingEvent)
            {
                EventSaveAttempts++;

                if (FailuresRemaining > 0)
                {
                    FailuresRemaining--;
                    throw new DbUpdateException("Simulated transient event-save failure.");
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    private List<RejectedEvent> ReadRejectedEvents()
    {
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>().RejectedEvents.ToList();
    }

}
