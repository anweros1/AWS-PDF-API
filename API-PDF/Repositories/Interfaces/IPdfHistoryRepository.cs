using API_PDF.Models.Entities;

namespace API_PDF.Repositories.Interfaces;

/// <summary>
/// Repository for managing PDF history records
/// </summary>
public interface IPdfHistoryRepository
{
    /// <summary>
    /// Add a new PDF history record
    /// </summary>
    Task<PdfHistory> AddAsync(PdfHistory history, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing PDF history record
    /// </summary>
    Task<PdfHistory> UpdateAsync(PdfHistory history, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get PDF history by GUID
    /// </summary>
    Task<PdfHistory?> GetByGuidAsync(string pdfGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all PDF histories for an application
    /// </summary>
    Task<List<PdfHistory>> GetByApplicationAsync(string applicationName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a PDF exists
    /// </summary>
    Task<bool> ExistsAsync(string pdfGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete a PDF history record
    /// </summary>
    Task<bool> SoftDeleteAsync(string pdfGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get merged PDFs (where IsMerged = true)
    /// </summary>
    Task<List<PdfHistory>> GetMergedPdfsAsync(CancellationToken cancellationToken = default);
}
