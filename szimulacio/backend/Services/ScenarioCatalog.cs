using System.Text.Json;
using System.Text.Json.Serialization;
using backend.Models;

namespace backend.Services;

public class ScenarioCatalog
{
    private readonly Dictionary<string, Scenario> _scenarios;

    public ScenarioCatalog(IWebHostEnvironment environment)
    {
        var directory = Path.Combine(environment.ContentRootPath, "Scenarios");

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException(
                $"Scenario directory was not found: {directory}");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());

        _scenarios = new Dictionary<string, Scenario>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var file in Directory.GetFiles(directory, "*.json"))
        {
            var json = File.ReadAllText(file);
            var scenario = JsonSerializer.Deserialize<Scenario>(json, options);

            if (scenario is null)
            {
                throw new InvalidOperationException(
                    $"Could not read scenario file: {file}");
            }

            if (string.IsNullOrWhiteSpace(scenario.Id))
            {
                throw new InvalidOperationException(
                    $"Scenario in {file} has no id.");
            }

            if (!_scenarios.TryAdd(scenario.Id, scenario))
            {
                throw new InvalidOperationException(
                    $"Duplicate scenario id: {scenario.Id}");
            }
        }
    }

    public IReadOnlyCollection<Scenario> GetAll()
    {
        return _scenarios.Values.ToArray();
    }

    public bool TryGet(string id, out Scenario? scenario)
    {
        return _scenarios.TryGetValue(id, out scenario);
    }
}