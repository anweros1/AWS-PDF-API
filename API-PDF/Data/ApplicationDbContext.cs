using API_PDF.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace API_PDF.Data;

/// <summary>
/// Application database context for PDF API
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// API call logs for tracking all requests
    /// </summary>
    public DbSet<ApiCallLog> ApiCallLogs { get; set; } = null!;

    /// <summary>
    /// PDF history for tracking PDF operations and metadata
    /// </summary>
    public DbSet<PdfHistory> PdfHistories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure ApiCallLog
        ConfigureApiCallLog(modelBuilder);

        // Configure PdfHistory
        ConfigurePdfHistory(modelBuilder);
    }

    private void ConfigureApiCallLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiCallLog>(entity =>
        {
            entity.ToTable("ApiCallLogs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.PdfGuid)
                .IsRequired()
                .HasMaxLength(36);

            entity.Property(e => e.ApplicationName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Endpoint)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.HttpMethod)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.RequestBody)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.ResponseBody)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.ErrorMessage)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.ClientIpAddress)
                .HasMaxLength(45);

            entity.Property(e => e.Timestamp)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes for performance
            entity.HasIndex(e => e.PdfGuid)
                .HasDatabaseName("IX_ApiCallLogs_PdfGuid");

            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("IX_ApiCallLogs_Timestamp");

            entity.HasIndex(e => new { e.PdfGuid, e.Timestamp })
                .HasDatabaseName("IX_ApiCallLogs_PdfGuid_Timestamp");

            entity.HasIndex(e => e.ApplicationName)
                .HasDatabaseName("IX_ApiCallLogs_ApplicationName");

            entity.HasIndex(e => e.IsSuccess)
                .HasDatabaseName("IX_ApiCallLogs_IsSuccess");
        });
    }

    private void ConfigurePdfHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PdfHistory>(entity =>
        {
            entity.ToTable("PdfHistories");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.PdfGuid)
                .IsRequired()
                .HasMaxLength(36);

            entity.Property(e => e.ApplicationName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.OriginalUrl)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.S3Url)
                .HasMaxLength(2000);

            entity.Property(e => e.S3BucketName)
                .HasMaxLength(200);

            entity.Property(e => e.S3ObjectKey)
                .HasMaxLength(500);

            entity.Property(e => e.LocalFilePath)
                .HasMaxLength(1000);

            entity.Property(e => e.Keywords)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.Bookmarks)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.Variables)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.SourcePdfGuids)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes for performance
            entity.HasIndex(e => e.PdfGuid)
                .IsUnique()
                .HasDatabaseName("IX_PdfHistories_PdfGuid");

            entity.HasIndex(e => e.ApplicationName)
                .HasDatabaseName("IX_PdfHistories_ApplicationName");

            entity.HasIndex(e => e.IsStoredInS3)
                .HasDatabaseName("IX_PdfHistories_IsStoredInS3");

            entity.HasIndex(e => e.IsMerged)
                .HasDatabaseName("IX_PdfHistories_IsMerged");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_PdfHistories_IsDeleted");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_PdfHistories_CreatedAt");

            entity.HasIndex(e => new { e.IsDeleted, e.PdfGuid })
                .HasDatabaseName("IX_PdfHistories_IsDeleted_PdfGuid");
        });
    }
}
