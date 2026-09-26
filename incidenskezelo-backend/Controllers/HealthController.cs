using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet(Name = "GetHealth")]
        public IActionResult Get()
        {
            return Ok(new { status = "ok" });
        }
    }
}
