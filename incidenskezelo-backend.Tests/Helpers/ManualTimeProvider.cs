namespace incidenskezelo_backend.Tests.Helpers;

public class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _now = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now += by;
}
