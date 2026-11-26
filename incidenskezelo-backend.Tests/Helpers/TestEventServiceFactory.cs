using incidenskezelo_backend.Data;
using incidenskezelo_backend.Services;
using incidenskezelo_backend.Services.IncidentAutomation;
using Microsoft.Extensions.Logging.Abstractions;

namespace incidenskezelo_backend.Tests.Helpers;

public static class TestEventServiceFactory
{
    public static EventService Create(AppDbContext dbContext) =>
        new(
            dbContext,
            NullLogger<EventService>.Instance,
            new IncidentCorrelationService(dbContext),
            new AutomaticIncidentService(dbContext, TimeProvider.System));
}