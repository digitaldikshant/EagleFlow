using System.ComponentModel.DataAnnotations;

namespace EagleFlow.Models;

public class AdminUser
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(30)]
    public string? MobileNumber { get; set; }

    public bool IsActive { get; set; } = true;
}
