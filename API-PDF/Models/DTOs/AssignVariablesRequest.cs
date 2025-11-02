using System.ComponentModel.DataAnnotations;

namespace API_PDF.Models.DTOs;

/// <summary>
/// Request to assign variables to PDF form fields
/// </summary>
public class AssignVariablesRequest
{
    /// <summary>
    /// GUID of the PDF file
    /// </summary>
    [Required(ErrorMessage = "PDF GUID is required")]
    [MaxLength(36)]
    public string PdfGuid { get; set; } = string.Empty;

    /// <summary>
    /// Dictionary of field names and their values
    /// </summary>
    [Required(ErrorMessage = "At least one variable is required")]
    [MinLength(1, ErrorMessage = "At least one variable is required")]
    public Dictionary<string, string> Variables { get; set; } = new();
}
