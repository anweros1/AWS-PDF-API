using API_PDF.Models.Entities;

namespace API_PDF.Repositories.Interfaces;

/// <summary>
/// Repository for managing API call logs
/// </summary>
public interface ILogRepository
{
    /// <summary>
    /// Add a new API call log entry
    /// </summary>
    Task<ApiCallLog> AddLogAsync(ApiCallLog log, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all logs for a specific PDF GUID
    /// </summary>
    Task<List<ApiCallLog>> GetLogsByPdfGuidAsync(string pdfGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all logs for a specific application
    /// </summary>
    Task<List<ApiCallLog>> GetLogsByApplicationAsync(string applicationName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get logs within a date range
    /// </summary>
    Task<List<ApiCallLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get failed API calls
    /// </summary>
    Task<List<ApiCallLog>> GetFailedLogsAsync(int count = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get log statistics for a PDF GUID
    /// </summary>
    Task<(int TotalCalls, int SuccessfulCalls, int FailedCalls, long AverageDurationMs)> GetLogStatisticsAsync(
        string pdfGuid, 
        CancellationToken cancellationToken = default);
}
