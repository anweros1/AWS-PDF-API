using API_PDF.Data;
using API_PDF.Models.Entities;
using API_PDF.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_PDF.Repositories;

/// <summary>
/// Repository implementation for PDF history records
/// </summary>
public class PdfHistoryRepository : IPdfHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public PdfHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PdfHistory> AddAsync(PdfHistory history, CancellationToken cancellationToken = default)
    {
        _context.PdfHistories.Add(history);
        await _context.SaveChangesAsync(cancellationToken);
        return history;
    }

    public async Task<PdfHistory> UpdateAsync(PdfHistory history, CancellationToken cancellationToken = default)
    {
        history.UpdatedAt = DateTime.UtcNow;
        _context.PdfHistories.Update(history);
        await _context.SaveChangesAsync(cancellationToken);
        return history;
    }

    public async Task<PdfHistory?> GetByGuidAsync(string pdfGuid, CancellationToken cancellationToken = default)
    {
        return await _context.PdfHistories
            .FirstOrDefaultAsync(h => h.PdfGuid == pdfGuid && !h.IsDeleted, cancellationToken);
    }

    public async Task<List<PdfHistory>> GetByApplicationAsync(
        string applicationName, 
        CancellationToken cancellationToken = default)
    {
        return await _context.PdfHistories
            .Where(h => h.ApplicationName == applicationName && !h.IsDeleted)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string pdfGuid, CancellationToken cancellationToken = default)
    {
        return await _context.PdfHistories
            .AnyAsync(h => h.PdfGuid == pdfGuid && !h.IsDeleted, cancellationToken);
    }

    public async Task<bool> SoftDeleteAsync(string pdfGuid, CancellationToken cancellationToken = default)
    {
        var history = await GetByGuidAsync(pdfGuid, cancellationToken);
        if (history == null)
        {
            return false;
        }

        history.IsDeleted = true;
        history.DeletedAt = DateTime.UtcNow;
        history.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<PdfHistory>> GetMergedPdfsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PdfHistories
            .Where(h => h.IsMerged && !h.IsDeleted)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
