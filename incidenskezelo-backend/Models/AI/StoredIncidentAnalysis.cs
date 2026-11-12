namespace incidenskezelo_backend.Models.AI;

public sealed class StoredIncidentAnalysis
{
    // Az Incident tábla belső int kulcsa, egyben ennek a táblának a PK-ja.
    public int IncidentId { get; set; }

    // A teljes IncidentAnalysisResult JSON-ként.
    public string ResultJson { get; set; } = string.Empty;

    public DateTime AnalyzedAtUtc { get; set; }
}