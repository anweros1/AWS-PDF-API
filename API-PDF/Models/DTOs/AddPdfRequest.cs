using System.ComponentModel.DataAnnotations;

namespace API_PDF.Models.DTOs;

/// <summary>
/// Request to add a new PDF to the system
/// </summary>
public class AddPdfRequest
{
    /// <summary>
    /// Name of the application making the request
    /// </summary>
    [Required(ErrorMessage = "Application name is required")]
    [MaxLength(200)]
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// URL to the PDF file provided by the application
    /// </summary>
    [Required(ErrorMessage = "File URL is required")]
    [MaxLength(2000)]
    [Url(ErrorMessage = "Invalid URL format")]
    public string FileUrl { get; set; } = string.Empty;
}
