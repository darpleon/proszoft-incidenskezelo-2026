using incidenskezelo_backend.Auth;
using incidenskezelo_backend.Controllers;
using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using incidenskezelo_backend.Models.Incident;
using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace incidenskezelo_backend.Tests.Controllers;

[TestClass]
public class IncidentValidationTests
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    [TestInitialize]
    public async Task Setup()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(IncidentsController).Assembly)
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(allowIntegerValues: false)));

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddScoped<IIncidentService, IncidentService>();
        builder.Services.AddScoped<EventService>();

        // A tesztek hitelesített operátorként érik el az incidens végpontot.
        builder.Services
            .AddAuthentication(TestAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName,
                _ => { });

        builder.Services.AddAuthorization(options =>
            options.AddPolicy(
                AuthPolicies.CanHandleIncidents,
                policy => policy.RequireRole(
                    nameof(UserRole.Operator),
                    nameof(UserRole.Admin))));

        _app = builder.Build();

        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();

        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    // Hibás cím esetén a controller 400-at ad, és nem jön létre incidens.
    [TestMethod]
    public async Task Create_BlankTitle_ReturnsBadRequestAndDoesNotCreateIncident()
    {
        using var response = await PostIncidentAsync(
            """{"title":"   ","description":"Érvényes leírás","priority":"High"}""");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(0, await CountIncidentsAsync());
    }

    // Hibás leírás esetén a controller 400-at ad, és nem jön létre incidens.
    [TestMethod]
    public async Task Create_BlankDescription_ReturnsBadRequestAndDoesNotCreateIncident()
    {
        using var response = await PostIncidentAsync(
            """{"title":"Érvényes cím","description":" ","priority":"High"}""");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(0, await CountIncidentsAsync());
    }

    // A maximális címhosszt meghaladó értéket a controller visszautasítja.
    [TestMethod]
    public async Task Create_TitleTooLong_ReturnsBadRequestAndDoesNotCreateIncident()
    {
        var title = new string('x', IncidentLimits.TitleMaxLength + 1);
        var body = System.Text.Json.JsonSerializer.Serialize(new
        {
            title,
            description = "Érvényes leírás",
            priority = "High"
        });

        using var response = await PostIncidentAsync(body);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(0, await CountIncidentsAsync());
    }

    // A maximális leíráshosszt meghaladó értéket a controller visszautasítja.
    [TestMethod]
    public async Task Create_DescriptionTooLong_ReturnsBadRequestAndDoesNotCreateIncident()
    {
        var description = new string('x', IncidentLimits.DescriptionMaxLength + 1);
        var body = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Érvényes cím",
            description,
            priority = "High"
        });

        using var response = await PostIncidentAsync(body);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(0, await CountIncidentsAsync());
    }

    // Hibás JSON esetén az ASP.NET model binding ad 400-at, az IncidentsController.Create action nem dolgozhat fel ilyen kérést.
    [TestMethod]
    public async Task Create_MalformedJson_ReturnsBadRequestAndDoesNotCreateIncident()
    {
        using var response = await PostIncidentAsync("{ invalid json");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(0, await CountIncidentsAsync());
    }

    private Task<HttpResponseMessage> PostIncidentAsync(string body)
    {
        return _client.PostAsync(
            "/api/incidents",
            new StringContent(body, Encoding.UTF8, "application/json"));
    }

    private async Task<int> CountIncidentsAsync()
    {
        await using var scope = _app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await dbContext.Incidents.CountAsync();
    }

    // A tesztek operátor felhasználóval lépnek be, így a 400-as választ a request validáció, nem a jogosultságvédelem adja.
    private sealed class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "Test";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(TokenClaims.UserId, "1"),
                new Claim(TokenClaims.Name, "Teszt operátor"),
                new Claim(TokenClaims.Role, nameof(UserRole.Operator))
            };

            var identity = new ClaimsIdentity(
                claims,
                SchemeName,
                TokenClaims.Name,
                TokenClaims.Role);

            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}