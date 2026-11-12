using backend.Models;

namespace backend.Services;

public enum EventPublishStatus
{
    Published,
    Failed,
}

public record EventPublishResult(EventPublishStatus Status, string? Error = null)
{
    public bool IsPublished => Status == EventPublishStatus.Published;
}

public interface IEventPublisher
{
    Task<EventPublishResult> PublishAsync(
        SimulationEvent simulatedEvent,
        CancellationToken cancellationToken);
}
