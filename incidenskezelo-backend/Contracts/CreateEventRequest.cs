using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace incidenskezelo_backend.Contracts;

public class CreateEventRequest
{
    [Required, MaxLength(100)]
    public string SourceEventId { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string ServiceCode { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Severity { get; set; } = string.Empty;

    [Required]
    public DateTime? OccurredAtUtc { get; set; }

    [MaxLength(500)]
    public string? Summary { get; set; }

    public JsonElement? Payload { get; set; }
}
