using incidenskezelo_backend.Models.AI;

namespace incidenskezelo_backend.Services.AI;

public interface IAiService
{
    Task<AiTurnResponse> ExecuteTurnAsync(
        AiTurnRequest request,
        CancellationToken cancellationToken = default);
}
