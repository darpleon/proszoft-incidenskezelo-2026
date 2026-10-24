using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using incidenskezelo_backend.Models.Incident;
using incidenskezelo_backend.Services;
using incidenskezelo_backend.Tests.Helpers;

namespace incidenskezelo_backend.Tests.Services;

[TestClass]
public class IncidentServiceTests
{
    private AppDbContext _dbContext = null!;
    private IncidentService _incidentService = null!;
    private User _operator = null!;


    [TestInitialize]
    public void Setup()
    {
        _dbContext = TestDbContextFactory.Create();
        _incidentService = new IncidentService(_dbContext, TimeProvider.System);
        _operator = AddUser("Kovács Anna", UserRole.Operator);
    }


    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }


    [TestMethod]
    public async Task CreateAsync_WithActor_StoresActorOnCreation()
    {
        var incident = await CreateIncidentAsync();

        var history = await _incidentService.GetHistoryAsync(incident.Id, CancellationToken.None);

        var created = history!.Single();
        Assert.AreEqual("created", created.Kind);
        Assert.AreEqual(_operator.UserId, created.ActorUserId);
        Assert.AreEqual("Kovács Anna", created.ActorName);
    }


    [TestMethod]
    public async Task UpdateAssigneeAsync_ValidUser_SetsAssigneeAndHistory()
    {
        var incident = await CreateIncidentAsync();

        var result = await _incidentService.UpdateAssigneeAsync(incident.Id, _operator.UserId, 0, _operator.UserId, CancellationToken.None);

        Assert.AreEqual(_operator.UserId, result!.AssigneeUserId);
        Assert.AreEqual("Kovács Anna", result.AssigneeName);
        Assert.AreEqual(1, result.Revisions.Assignee);

        var history = await _incidentService.GetHistoryAsync(incident.Id, CancellationToken.None);
        var change = history!.Single(entry => entry.Kind == "assignee");
        Assert.IsNull(change.OldValue);
        Assert.AreEqual("Kovács Anna", change.NewValue);
        Assert.AreEqual(_operator.UserId, change.ActorUserId);
    }


    [TestMethod]
    public async Task UpdateAssigneeAsync_NullAssignee_RemovesAssignee()
    {
        var incident = await CreateIncidentAsync();
        await _incidentService.UpdateAssigneeAsync(incident.Id, _operator.UserId, 0, _operator.UserId, CancellationToken.None);

        var result = await _incidentService.UpdateAssigneeAsync(incident.Id, null, 1, _operator.UserId, CancellationToken.None);

        Assert.IsNull(result!.AssigneeUserId);
        Assert.AreEqual(2, result.Revisions.Assignee);
    }


    [TestMethod]
    [DataRow(UserRole.Pending, true, DisplayName = "pending user")]
    [DataRow(UserRole.Operator, false, DisplayName = "inactive user")]
    public async Task UpdateAssigneeAsync_NotAssignableUser_ThrowsArgumentException(UserRole role, bool isActive)
    {
        var incident = await CreateIncidentAsync();
        var user = AddUser("Tóth Bence", role, isActive);

        await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
            _incidentService.UpdateAssigneeAsync(incident.Id, user.UserId, 0, _operator.UserId, CancellationToken.None));
    }


    [TestMethod]
    public async Task UpdateAssigneeAsync_StaleRevision_ThrowsRevisionConflict()
    {
        var incident = await CreateIncidentAsync();
        await _incidentService.UpdateAssigneeAsync(incident.Id, _operator.UserId, 0, _operator.UserId, CancellationToken.None);

        await Assert.ThrowsExactlyAsync<RevisionConflictException>(() =>
            _incidentService.UpdateAssigneeAsync(incident.Id, null, 0, _operator.UserId, CancellationToken.None));
    }


    private async Task<IncidentResponse> CreateIncidentAsync()
    {
        var request = new CreateIncidentRequest("Database connection lost", "postgres-main refuses connections.", IncidentPriority.High);

        return await _incidentService.CreateAsync(request, _operator.UserId, CancellationToken.None);
    }


    private User AddUser(string name, UserRole role, bool isActive = true)
    {
        var user = new User
        {
            Name = name,
            Email = $"{Guid.NewGuid()}@example.com",
            Role = role,
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow,
        };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        return user;
    }
}
