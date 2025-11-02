namespace API_PDF.Models.DTOs;

/// <summary>
/// Response after adding a PDF to the system
/// </summary>
public class AddPdfResponse
{
    /// <summary>
    /// GUID of the PDF file
    /// </summary>
    public string PdfGuid { get; set; } = string.Empty;

    /// <summary>
    /// URL to access the PDF from S3
    /// </summary>
    public string S3Url { get; set; } = string.Empty;

    /// <summary>
    /// Whether the file was stored in S3 or locally
    /// </summary>
    public bool IsStoredInS3 { get; set; }

    /// <summary>
    /// History information about the PDF
    /// </summary>
    public PdfHistoryDto History { get; set; } = new();

    /// <summary>
    /// Success status
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
