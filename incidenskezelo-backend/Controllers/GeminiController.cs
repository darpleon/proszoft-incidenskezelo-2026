using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeminiController : ControllerBase
{
    private readonly IAiService _aiService;

    public GeminiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost]
    public async Task<IActionResult> Generate(
        [FromBody] string prompt,
        CancellationToken cancellationToken)
    {
        var result = await _aiService.GenerateAsync(
            prompt,
            cancellationToken);

        return Ok(result);
    }
}