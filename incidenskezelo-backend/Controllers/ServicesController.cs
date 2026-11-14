using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController(ServiceCatalogService serviceCatalogService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ServiceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceResponse>>> GetActive(CancellationToken ct)
    {
        return await serviceCatalogService.GetActiveAsync(ct);
    }
}
