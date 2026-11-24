using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace incidenskezelo_backend.Services;

public class RejectedEventService(AppDbContext dbContext, TimeProvider timeProvider)
{
    public async Task RecordAsync(
        RejectedEventSource source,
        RejectedEventReason reason,
        string error,
        string rawPayload,
        CancellationToken cancellationToken)
    {
        dbContext.RejectedEvents.Add(new RejectedEvent
        {
            Source = source,
            Reason = reason,
            Error = error,
            RawPayload = rawPayload,
            ReceivedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }


    public async Task<List<RejectedEventResponse>> GetLatestAsync(int count, CancellationToken cancellationToken)
    {
        return await dbContext.RejectedEvents.AsNoTracking()
            .OrderByDescending(rejected => rejected.RejectedEventId)
            .Take(count)
            .Select(rejected => new RejectedEventResponse
            {
                RejectedEventId = rejected.RejectedEventId,
                Source = rejected.Source,
                Reason = rejected.Reason,
                Error = rejected.Error,
                RawPayload = rejected.RawPayload,
                ReceivedAtUtc = rejected.ReceivedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }
}
