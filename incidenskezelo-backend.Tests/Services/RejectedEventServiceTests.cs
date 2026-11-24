using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using incidenskezelo_backend.Services;
using incidenskezelo_backend.Tests.Helpers;

namespace incidenskezelo_backend.Tests.Services;

[TestClass]
public class RejectedEventServiceTests
{
    private AppDbContext _dbContext = null!;
    private RejectedEventService _rejectedEventService = null!;


    [TestInitialize]
    public void Setup()
    {
        _dbContext = TestDbContextFactory.Create();
        _rejectedEventService = new RejectedEventService(_dbContext, TimeProvider.System);
    }


    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }


    [TestMethod]
    public async Task GetLatestAsync_SeveralRejectedEvents_ReturnsNewestFirstUpToCount()
    {
        await _rejectedEventService.RecordAsync(RejectedEventSource.Queue, RejectedEventReason.Malformed, "first", "{", CancellationToken.None);
        await _rejectedEventService.RecordAsync(RejectedEventSource.Http, RejectedEventReason.Invalid, "second", "{}", CancellationToken.None);
        await _rejectedEventService.RecordAsync(RejectedEventSource.Queue, RejectedEventReason.Invalid, "third", "{}", CancellationToken.None);

        var latest = await _rejectedEventService.GetLatestAsync(2, CancellationToken.None);

        Assert.AreEqual(2, latest.Count);
        Assert.AreEqual("third", latest[0].Error);
        Assert.AreEqual("second", latest[1].Error);
    }
}
