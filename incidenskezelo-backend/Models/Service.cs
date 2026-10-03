namespace incidenskezelo_backend.Models;

public class Service
{
    public int ServiceId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ServiceKind Kind { get; set; }

    public bool IsActive { get; set; } = true;
}
