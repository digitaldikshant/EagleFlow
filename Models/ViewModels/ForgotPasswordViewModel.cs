using System.ComponentModel.DataAnnotations;

namespace EagleFlow.Models.ViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Admin Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "OTP Delivery")]
    public string Channel { get; set; } = "email";

    [Display(Name = "Mobile Number")]
    public string? MobileNumber { get; set; }
}
