using System.ComponentModel.DataAnnotations;

namespace incidenskezelo_backend.Contracts;

public class UpdateActiveRequest
{
    [Required]
    public bool? IsActive { get; set; }
}
