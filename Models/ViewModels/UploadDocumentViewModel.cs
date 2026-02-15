using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EagleFlow.Models.ViewModels;

public class UploadDocumentViewModel
{
    [Required]
    public IFormFile? File { get; set; }
}
