using incidenskezelo_backend.Data;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet(Name = "GetHealth")]
        public async Task<IActionResult> Get()
        {
            var databaseConnected = await dbContext.Database.CanConnectAsync();
            return Ok(new { status = "ok", database = databaseConnected ? "ok" : "unavailable" });
        }
    }
}
