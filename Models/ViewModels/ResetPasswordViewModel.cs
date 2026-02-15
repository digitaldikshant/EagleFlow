using System.ComponentModel.DataAnnotations;

namespace EagleFlow.Models.ViewModels;

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Admin Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "OTP")]
    [StringLength(6, MinimumLength = 6)]
    public string OtpCode { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
