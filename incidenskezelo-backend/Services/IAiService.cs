namespace incidenskezelo_backend.Services;

public interface IAiService
{
    Task<string> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}