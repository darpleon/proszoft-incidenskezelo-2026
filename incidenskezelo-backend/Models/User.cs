namespace incidenskezelo_backend.Models;

public class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Pending;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
}
