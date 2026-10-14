using System.ComponentModel.DataAnnotations;

namespace incidenskezelo_backend.Contracts;

public class UpdateRoleRequest
{
    [Required, MaxLength(20)]
    public string Role { get; set; } = string.Empty;
}
