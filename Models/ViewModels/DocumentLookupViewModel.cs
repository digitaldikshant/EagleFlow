using System.ComponentModel.DataAnnotations;

namespace EagleFlow.Models.ViewModels;

public class DocumentLookupViewModel
{
    [Required]
    [Display(Name = "Document Number")]
    public string DocumentNumber { get; set; } = string.Empty;
}
