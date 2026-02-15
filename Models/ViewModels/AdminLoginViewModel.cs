using System.ComponentModel.DataAnnotations;

namespace EagleFlow.Models.ViewModels;

public class AdminLoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Admin Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
