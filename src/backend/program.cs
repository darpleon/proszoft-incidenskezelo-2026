using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "PoC Backend is up!");

app.MapGet("/db-check", async (IConfiguration config) =>
{
    var connStr = config.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connStr))
    {
        return Results.Problem("Connection string missing.");
    }

    try
    {
        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("SELECT @@VERSION;", conn);
        var version = await cmd.ExecuteScalarAsync();

        return Results.Ok(new
        {
            status = "Connected successfully!",
            sqlVersion = version?.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to connect to database: {ex.Message}");
    }
});

app.Run();