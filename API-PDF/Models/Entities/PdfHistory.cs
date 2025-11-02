using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PDF.Models.Entities;

/// <summary>
/// Entity for tracking PDF file history and metadata
/// </summary>
public class PdfHistory
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// GUID of the PDF file (unique identifier)
    /// </summary>
    [Required]
    [MaxLength(36)]
    public string PdfGuid { get; set; } = string.Empty;

    /// <summary>
    /// Name of the application that uploaded the PDF
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Original URL provided by the application
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>
    /// S3 URL where the PDF is stored
    /// </summary>
    [MaxLength(2000)]
    public string? S3Url { get; set; }

    /// <summary>
    /// S3 bucket name
    /// </summary>
    [MaxLength(200)]
    public string? S3BucketName { get; set; }

    /// <summary>
    /// S3 object key
    /// </summary>
    [MaxLength(500)]
    public string? S3ObjectKey { get; set; }

    /// <summary>
    /// Local file path (fallback when S3 is unavailable)
    /// </summary>
    [MaxLength(1000)]
    public string? LocalFilePath { get; set; }

    /// <summary>
    /// Whether the file is stored in S3 or locally
    /// </summary>
    public bool IsStoredInS3 { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Number of pages in the PDF
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// JSON array of keywords added to the PDF
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Keywords { get; set; }

    /// <summary>
    /// JSON array of bookmarks added to the PDF
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Bookmarks { get; set; }

    /// <summary>
    /// JSON object of variables assigned to the PDF
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Variables { get; set; }

    /// <summary>
    /// Whether this PDF is a result of a merge operation
    /// </summary>
    public bool IsMerged { get; set; }

    /// <summary>
    /// JSON array of source PDF GUIDs if this is a merged PDF
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? SourcePdfGuids { get; set; }

    /// <summary>
    /// When the PDF was first uploaded
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last time the PDF was modified
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the PDF has been deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the PDF was deleted (soft delete)
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
