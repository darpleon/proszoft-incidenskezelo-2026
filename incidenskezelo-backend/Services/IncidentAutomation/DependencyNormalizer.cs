using System.Text.Json;

namespace incidenskezelo_backend.Services.IncidentAutomation;

public static class DependencyNormalizer
{
    private static readonly Dictionary<string, string> Aliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["mssql"] = "postgres-main",
            ["database"] = "postgres-main",
            ["external-service"] = "payment-gw",
        };

    public static string? FromPayload(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(payloadJson);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var values = new[]
                {
                    ReadString(document.RootElement, "dependency"),
                    ReadString(document.RootElement, "suspectedDependency"),
                }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .OfType<string>()
                .Select(value => value.Trim())
                .Where(value => value.Length > 0)
                .Select(Normalize)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            // Conflicting dependency hints are ambiguous; don't guess.
            return values.Length == 1 ? values[0] : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(
                    property.Name,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase)
                && property.Value.ValueKind == JsonValueKind.String)
            {
                return property.Value.GetString();
            }
        }

        return null;
    }

    private static string Normalize(string value)
    {
        var trimmed = value.Trim();

        return Aliases.TryGetValue(trimmed, out var canonical)
            ? canonical
            : trimmed.ToLowerInvariant();
    }
}