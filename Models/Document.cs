using System.ComponentModel.DataAnnotations;

namespace EagleFlow.Models;

public class Document
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
}
