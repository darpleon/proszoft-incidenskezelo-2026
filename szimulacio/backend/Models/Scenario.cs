namespace backend.Models;

public class Scenario
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<ScenarioStep> Steps { get; set; } = [];
}
