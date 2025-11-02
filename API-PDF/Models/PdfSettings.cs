using System.ComponentModel.DataAnnotations;

namespace API_PDF.Models;

/// <summary>
/// PDF processing configuration settings
/// </summary>
public class PdfSettings
{
    public const string SectionName = "PdfSettings";

    /// <summary>
    /// Temporary folder for PDF processing
    /// </summary>
    [Required(ErrorMessage = "Temp folder path is required")]
    public string TempFolder { get; set; } = "C:\\Temp\\PDFs";

    /// <summary>
    /// Local fallback folder when S3 is unavailable
    /// </summary>
    [Required(ErrorMessage = "Local fallback folder path is required")]
    public string LocalFallbackFolder { get; set; } = "C:\\PDFs\\Fallback";

    /// <summary>
    /// Maximum file size in MB
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Max file size must be between 1 and 1000 MB")]
    public int MaxFileSizeMb { get; set; } = 100;

    /// <summary>
    /// Whether to automatically create folders if they don't exist
    /// </summary>
    public bool AutoCreateFolders { get; set; } = true;
}
