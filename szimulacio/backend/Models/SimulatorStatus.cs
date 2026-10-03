namespace backend.Models;

public class SimulatorStatus
{
    public string InstanceId { get; set; } = string.Empty;

    public bool Running { get; set; }

    public int EventsGenerated { get; set; }

    public int IntervalSeconds { get; set; }
}