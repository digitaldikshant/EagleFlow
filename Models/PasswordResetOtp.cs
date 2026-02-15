using System.ComponentModel.DataAnnotations;

namespace EagleFlow.Models;

public class PasswordResetOtp
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string OtpCode { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Channel { get; set; } = "email";

    [StringLength(320)]
    public string? Destination { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
