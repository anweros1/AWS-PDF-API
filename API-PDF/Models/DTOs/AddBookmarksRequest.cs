using System.ComponentModel.DataAnnotations;

namespace API_PDF.Models.DTOs;

/// <summary>
/// Request to add bookmarks to a PDF
/// </summary>
public class AddBookmarksRequest
{
    /// <summary>
    /// GUID of the PDF file
    /// </summary>
    [Required(ErrorMessage = "PDF GUID is required")]
    [MaxLength(36)]
    public string PdfGuid { get; set; } = string.Empty;

    /// <summary>
    /// List of bookmarks to add
    /// </summary>
    [Required(ErrorMessage = "At least one bookmark is required")]
    [MinLength(1, ErrorMessage = "At least one bookmark is required")]
    public List<BookmarkDto> Bookmarks { get; set; } = new();
}

/// <summary>
/// Bookmark data transfer object
/// </summary>
public class BookmarkDto
{
    /// <summary>
    /// Bookmark title/value
    /// </summary>
    [Required(ErrorMessage = "Bookmark value is required")]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Page number where the bookmark points (1-based)
    /// </summary>
    [Required(ErrorMessage = "Page number is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; }
}
