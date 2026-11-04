namespace incidenskezelo_backend.Configuration;

public class EventIngestOptions
{
    public int RetryAttempts { get; set; } = 3;

    public bool DirectHttpEnabled { get; set; }
}
