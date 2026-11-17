using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace incidenskezelo_backend.Services;

public class ServiceCatalogService(AppDbContext context)
{
    public async Task<List<ServiceResponse>> GetActiveAsync(CancellationToken ct)
    {
        return await context.Services.AsNoTracking()
            .Where(service => service.IsActive)
            .OrderBy(service => service.Code)
            .Select(service => new ServiceResponse { Code = service.Code, Name = service.Name })
            .ToListAsync(ct);
    }
}
