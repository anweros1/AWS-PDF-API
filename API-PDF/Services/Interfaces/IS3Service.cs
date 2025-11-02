namespace API_PDF.Services.Interfaces;

/// <summary>
/// Service for managing PDF files in AWS S3 with local fallback
/// </summary>
public interface IS3Service
{
    /// <summary>
    /// Upload a PDF file to S3 or local storage
    /// </summary>
    /// <param name="filePath">Local file path to upload</param>
    /// <param name="pdfGuid">GUID identifier for the PDF</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>S3 URL if uploaded to S3, local path if fallback, and flag indicating storage location</returns>
    Task<(string Url, bool IsStoredInS3)> UploadPdfAsync(string filePath, string pdfGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Download a PDF file from S3 or local storage to a temporary location
    /// </summary>
    /// <param name="pdfGuid">GUID identifier for the PDF</param>
    /// <param name="destinationPath">Where to save the downloaded file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if download successful</returns>
    Task<bool> DownloadPdfAsync(string pdfGuid, string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a PDF file from S3 or local storage
    /// </summary>
    /// <param name="pdfGuid">GUID identifier for the PDF</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion successful</returns>
    Task<bool> DeletePdfAsync(string pdfGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a PDF exists in S3 or local storage
    /// </summary>
    /// <param name="pdfGuid">GUID identifier for the PDF</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file exists</returns>
    Task<bool> PdfExistsAsync(string pdfGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the URL or path to access a PDF
    /// </summary>
    /// <param name="pdfGuid">GUID identifier for the PDF</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>URL/path and flag indicating if it's in S3</returns>
    Task<(string? Url, bool IsStoredInS3)> GetPdfUrlAsync(string pdfGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if S3 service is available
    /// </summary>
    /// <returns>True if S3 is accessible</returns>
    Task<bool> IsS3AvailableAsync();
}
