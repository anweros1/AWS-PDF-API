namespace API_PDF.Models.DTOs;

/// <summary>
/// Generic response for PDF operations
/// </summary>
public class PdfOperationResponse
{
    /// <summary>
    /// GUID of the PDF file
    /// </summary>
    public string PdfGuid { get; set; } = string.Empty;

    /// <summary>
    /// Success status
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// URL to access the PDF from S3
    /// </summary>
    public string? S3Url { get; set; }

    /// <summary>
    /// Updated history information
    /// </summary>
    public PdfHistoryDto? History { get; set; }

    /// <summary>
    /// Error details if operation failed
    /// </summary>
    public string? ErrorDetails { get; set; }
}
