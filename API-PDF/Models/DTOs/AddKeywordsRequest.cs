using System.ComponentModel.DataAnnotations;

namespace API_PDF.Models.DTOs;

/// <summary>
/// Request to add keywords to a PDF (order is preserved)
/// </summary>
public class AddKeywordsRequest
{
    /// <summary>
    /// GUID of the PDF file
    /// </summary>
    [Required(ErrorMessage = "PDF GUID is required")]
    [MaxLength(36)]
    public string PdfGuid { get; set; } = string.Empty;

    /// <summary>
    /// List of keywords to add (order will be preserved)
    /// </summary>
    [Required(ErrorMessage = "At least one keyword is required")]
    [MinLength(1, ErrorMessage = "At least one keyword is required")]
    public List<string> Keywords { get; set; } = new();
}
