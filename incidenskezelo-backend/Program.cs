using System.Text;
using System.Text.Json.Serialization;
using incidenskezelo_backend.Auth;
using incidenskezelo_backend.Data;
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Models;
using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<BearerSecuritySchemeTransformer>();
});
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing. Set the ConnectionStrings__DefaultConnection environment variable.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
    connectionString,
    sqlOptions => sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 10,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null)));
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod()));

builder.Services.Configure<GeminiOptions>(
    builder.Configuration.GetSection("Gemini"));

builder.Services.AddHttpClient<IAiService, GeminiService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<IIncidentService, IncidentService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddSingleton(TimeProvider.System);

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (jwtOptions.Key.Length < JwtOptions.MinKeyLength)
{
    throw new InvalidOperationException(
        $"Jwt:Key is missing or shorter than {JwtOptions.MinKeyLength} characters. Set it in user secrets or the Jwt__Key environment variable.");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
        NameClaimType = TokenClaims.Name,
        RoleClaimType = TokenClaims.Role,
    };
});

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
    .AddPolicy(AuthPolicies.CanHandleIncidents, policy => policy.RequireRole(nameof(UserRole.Operator), nameof(UserRole.Admin)))
    .AddPolicy(AuthPolicies.AdminOnly, policy => policy.RequireRole(nameof(UserRole.Admin)));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));
}

// Off in containers: TLS termination is a reverse proxy's job here.
// app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
