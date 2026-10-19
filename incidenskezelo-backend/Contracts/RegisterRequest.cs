using System.ComponentModel.DataAnnotations;
using incidenskezelo_backend.Models;

namespace incidenskezelo_backend.Contracts;

public class RegisterRequest
{
    [Required, MaxLength(UserLimits.NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(UserLimits.EmailMaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(UserLimits.PasswordMinLength, ErrorMessage = "Password must be at least 10 characters.")]
    [MaxLength(UserLimits.PasswordMaxLength, ErrorMessage = "Password must be at most 128 characters.")]
    public string Password { get; set; } = string.Empty;
}
