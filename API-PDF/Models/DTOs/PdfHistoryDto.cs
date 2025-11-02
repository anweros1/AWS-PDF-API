namespace API_PDF.Models.DTOs;

/// <summary>
/// DTO for PDF history information
/// </summary>
public class PdfHistoryDto
{
    /// <summary>
    /// GUID of the PDF file
    /// </summary>
    public string PdfGuid { get; set; } = string.Empty;

    /// <summary>
    /// Application that uploaded the PDF
    /// </summary>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Original URL provided
    /// </summary>
    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>
    /// S3 URL where the PDF is stored
    /// </summary>
    public string? S3Url { get; set; }

    /// <summary>
    /// Whether stored in S3 or locally
    /// </summary>
    public bool IsStoredInS3 { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Number of pages
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// Keywords added to the PDF
    /// </summary>
    public List<string> Keywords { get; set; } = new();

    /// <summary>
    /// Bookmarks added to the PDF
    /// </summary>
    public List<BookmarkDto> Bookmarks { get; set; } = new();

    /// <summary>
    /// Variables assigned to the PDF
    /// </summary>
    public Dictionary<string, string> Variables { get; set; } = new();

    /// <summary>
    /// Whether this is a merged PDF
    /// </summary>
    public bool IsMerged { get; set; }

    /// <summary>
    /// Source PDF GUIDs if merged
    /// </summary>
    public List<string> SourcePdfGuids { get; set; } = new();

    /// <summary>
    /// When the PDF was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
