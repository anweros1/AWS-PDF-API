using API_PDF.Data;
using API_PDF.Models.Entities;
using API_PDF.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_PDF.Repositories;

/// <summary>
/// Repository implementation for API call logs
/// </summary>
public class LogRepository : ILogRepository
{
    private readonly ApplicationDbContext _context;

    public LogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiCallLog> AddLogAsync(ApiCallLog log, CancellationToken cancellationToken = default)
    {
        _context.ApiCallLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
        return log;
    }

    public async Task<List<ApiCallLog>> GetLogsByPdfGuidAsync(string pdfGuid, CancellationToken cancellationToken = default)
    {
        return await _context.ApiCallLogs
            .Where(l => l.PdfGuid == pdfGuid)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ApiCallLog>> GetLogsByApplicationAsync(string applicationName, CancellationToken cancellationToken = default)
    {
        return await _context.ApiCallLogs
            .Where(l => l.ApplicationName == applicationName)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ApiCallLog>> GetLogsByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.ApiCallLogs
            .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ApiCallLog>> GetFailedLogsAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        return await _context.ApiCallLogs
            .Where(l => !l.IsSuccess)
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<(int TotalCalls, int SuccessfulCalls, int FailedCalls, long AverageDurationMs)> GetLogStatisticsAsync(
        string pdfGuid, 
        CancellationToken cancellationToken = default)
    {
        var logs = await _context.ApiCallLogs
            .Where(l => l.PdfGuid == pdfGuid)
            .ToListAsync(cancellationToken);

        if (!logs.Any())
        {
            return (0, 0, 0, 0);
        }

        var totalCalls = logs.Count;
        var successfulCalls = logs.Count(l => l.IsSuccess);
        var failedCalls = logs.Count(l => !l.IsSuccess);
        var averageDuration = (long)logs.Average(l => l.DurationMs);

        return (totalCalls, successfulCalls, failedCalls, averageDuration);
    }
}
