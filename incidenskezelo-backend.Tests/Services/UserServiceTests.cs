using incidenskezelo_backend.Auth;
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using incidenskezelo_backend.Services;
using incidenskezelo_backend.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace incidenskezelo_backend.Tests.Services;

// A regisztrációs és belépési üzleti logikát vizsgáló unit tesztek.
// Az adatbázis tesztenként külön InMemory példány.
[TestClass]
public class UserServiceTests
{
    private AppDbContext _dbContext = null!;
    private UserService _userService = null!;

    [TestInitialize]
    public void Setup()
    {
        _dbContext = TestDbContextFactory.Create();

        // Tesztkulccsal beállított TokenService. Nem használ éles JWT-titkot.
        var tokenService = new TokenService(
            Options.Create(new JwtOptions
            {
                Key = new string('k', 32),
                Issuer = "test-issuer",
                Audience = "test-audience",
                ExpireHours = 1
            }),
            TimeProvider.System);

        // A UserService valódi üzleti logikáját használja, csak az adatbázis és a naplózás tesztkörnyezetű.
        _userService = new UserService(
            _dbContext,
            tokenService,
            TimeProvider.System,
            NullLogger<UserService>.Instance);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }

    // Sikeres regisztrációkor a felhasználó Pending szereppel jön létre, az email címe normalizálódik, a jelszó pedig hash-elve tárolódik.
    [TestMethod]
    public async Task RegisterAsync_ValidRequest_CreatesPendingUserWithHashedPassword()
    {
        var request = CreateRegisterRequest();

        var result = await _userService.RegisterAsync(
            request,
            CancellationToken.None);

        Assert.AreEqual(UserActionStatus.Success, result.Status);
        Assert.IsNotNull(result.User);
        Assert.AreEqual(UserRole.Pending, result.User.Role);

        var savedUser = _dbContext.Users.Single();
        Assert.AreEqual("tester@example.com", savedUser.Email);
        Assert.AreNotEqual(request.Password, savedUser.PasswordHash);
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify(
            request.Password,
            savedUser.PasswordHash));
    }

    // Azonos email címmel nem hozható létre második felhasználó, a service Conflict eredménnyel tér vissza.
    [TestMethod]
    public async Task RegisterAsync_EmailAlreadyExists_ReturnsConflict()
    {
        await _userService.RegisterAsync(
            CreateRegisterRequest(),
            CancellationToken.None);

        var result = await _userService.RegisterAsync(
            CreateRegisterRequest(name: "Diferent User"),
            CancellationToken.None);

        Assert.AreEqual(UserActionStatus.Conflict, result.Status);
        Assert.AreEqual(1, _dbContext.Users.Count());
    }

    // Aktív és jóváhagyott felhasználó helyes adatokkal be tud lépni, JWT tokent kap, és az email-keresés kis- és nagybetűtől, 
    // valamint a szélső szóközöktől független.
    [TestMethod]
    public async Task LoginAsync_ApprovedActiveUser_ReturnsToken()
    {
        const string password = "Hard to decryp string chain";

        _dbContext.Users.Add(CreateUser(
            email: "admin@example.com",
            password: password,
            role: UserRole.Admin,
            isActive: true));

        await _dbContext.SaveChangesAsync();

        var result = await _userService.LoginAsync(
            new LoginRequest
            {
                Email = " ADMIN@example.com ",
                Password = password
            },
            CancellationToken.None);

        Assert.AreEqual(LoginStatus.Success, result.Status);
        Assert.IsNotNull(result.Response);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Response.Token));
        Assert.AreEqual(UserRole.Admin, result.Response.User.Role);
    }

    // Hibás jelszónál a service általános InvalidCredentials eredményt ad, és nem állít elő belépési választ vagy tokent.
    [TestMethod]
    public async Task LoginAsync_WrongPassword_ReturnsInvalidCredentials()
    {
        _dbContext.Users.Add(CreateUser(
            email: "admin@example.com",
            password: "Hard to decryp string chain",
            role: UserRole.Admin,
            isActive: true));

        await _dbContext.SaveChangesAsync();

        var result = await _userService.LoginAsync(
            new LoginRequest
            {
                Email = "admin@example.com",
                Password = "wrong password"
            },
            CancellationToken.None);

        Assert.AreEqual(LoginStatus.InvalidCredentials, result.Status);
        Assert.IsNull(result.Response);
    }

    // Helyes jelszóval sem enged belépni a rendszer, ha a felhasználó
    [TestMethod]
    public async Task LoginAsync_PendingUser_ReturnsPendingApproval()
    {
        const string password = "Hard to decryp string chain";

        _dbContext.Users.Add(CreateUser(
            email: "pending@example.com",
            password: password,
            role: UserRole.Pending,
            isActive: true));

        await _dbContext.SaveChangesAsync();

        var result = await _userService.LoginAsync(
            new LoginRequest
            {
                Email = "pending@example.com",
                Password = password
            },
            CancellationToken.None);

        Assert.AreEqual(LoginStatus.PendingApproval, result.Status);
        Assert.IsNull(result.Response);
    }

    // Helyes jelszóval sem enged belépni, ha a fiók inaktív;
    // a service Deactivated eredményt ad vissza token nélkül.
    [TestMethod]
    public async Task LoginAsync_DeactivatedUser_ReturnsDeactivated()
    {
        const string password = "Hard to decryp string chain";

        _dbContext.Users.Add(CreateUser(
            email: "inactive@example.com",
            password: password,
            role: UserRole.Operator,
            isActive: false));

        await _dbContext.SaveChangesAsync();

        var result = await _userService.LoginAsync(
            new LoginRequest
            {
                Email = "inactive@example.com",
                Password = password
            },
            CancellationToken.None);

        Assert.AreEqual(LoginStatus.Deactivated, result.Status);
        Assert.IsNull(result.Response);
    }

    // A regisztrációs tesztekhez létrehoz egy érvényes alapértelmezett kérést.
    // A name paraméterrel a duplikált email teszt különböző felhasználónevet ad.
    private static RegisterRequest CreateRegisterRequest(
        string name = "Mr Test User")
    {
        return new RegisterRequest
        {
            Name = name,
            Email = "tester@example.com",
            Password = "Hard to decryp string chain"
        };
    }

    // Tesztfelhasználót hoz létre hash-elt jelszóval, megadott szerepkörrel
    // és aktív állapottal; ezt használják a belépési tesztek előfeltételként.
    private static User CreateUser(
        string email,
        string password,
        UserRole role,
        bool isActive)
    {
        return new User
        {
            Name = "Mr Test User",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}