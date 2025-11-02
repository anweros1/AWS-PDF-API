using System.ComponentModel.DataAnnotations;

namespace API_PDF.Models.DTOs;

/// <summary>
/// Request to merge multiple PDFs into a single PDF
/// </summary>
public class MergePdfsRequest
{
    /// <summary>
    /// Name of the application making the request
    /// </summary>
    [Required(ErrorMessage = "Application name is required")]
    [MaxLength(200)]
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Ordered list of PDF GUIDs to merge
    /// </summary>
    [Required(ErrorMessage = "At least two PDFs are required for merging")]
    [MinLength(2, ErrorMessage = "At least two PDFs are required for merging")]
    public List<string> PdfGuids { get; set; } = new();
}
