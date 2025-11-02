using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PDF.Models.Entities;

/// <summary>
/// Entity for logging all API calls to the database
/// </summary>
public class ApiCallLog
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// GUID of the PDF file being operated on
    /// </summary>
    [Required]
    [MaxLength(36)]
    public string PdfGuid { get; set; } = string.Empty;

    /// <summary>
    /// Name of the application making the call
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Username of the person making the call
    /// </summary>
    [MaxLength(200)]
    public string? Username { get; set; }

    /// <summary>
    /// API endpoint that was called
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method (GET, POST, PUT, DELETE)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Request body (JSON serialized)
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? RequestBody { get; set; }

    /// <summary>
    /// Response status code
    /// </summary>
    public int? ResponseStatusCode { get; set; }

    /// <summary>
    /// Response body (JSON serialized)
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Error message if the call failed
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duration of the API call in milliseconds
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Timestamp when the call was made
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address of the client making the call
    /// </summary>
    [MaxLength(45)]
    public string? ClientIpAddress { get; set; }

    /// <summary>
    /// Whether the call was successful
    /// </summary>
    public bool IsSuccess { get; set; }
}
