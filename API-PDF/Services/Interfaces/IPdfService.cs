using API_PDF.Models.DTOs;

namespace API_PDF.Services.Interfaces;

/// <summary>
/// Service for PDF manipulation operations
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Add bookmarks to a PDF file
    /// </summary>
    /// <param name="pdfPath">Path to the PDF file</param>
    /// <param name="bookmarks">List of bookmarks with value and page number</param>
    /// <param name="outputPath">Path where the modified PDF will be saved</param>
    /// <returns>True if successful</returns>
    Task<bool> AddBookmarksAsync(string pdfPath, List<BookmarkDto> bookmarks, string outputPath);

    /// <summary>
    /// Assign values to form fields (variables) in a PDF
    /// </summary>
    /// <param name="pdfPath">Path to the PDF file</param>
    /// <param name="variables">Dictionary of field names and values</param>
    /// <param name="outputPath">Path where the modified PDF will be saved</param>
    /// <returns>True if successful</returns>
    Task<bool> AssignVariablesAsync(string pdfPath, Dictionary<string, string> variables, string outputPath);

    /// <summary>
    /// Add keywords to a PDF (order is preserved)
    /// </summary>
    /// <param name="pdfPath">Path to the PDF file</param>
    /// <param name="keywords">List of keywords in the desired order</param>
    /// <param name="outputPath">Path where the modified PDF will be saved</param>
    /// <returns>True if successful</returns>
    Task<bool> AddKeywordsAsync(string pdfPath, List<string> keywords, string outputPath);

    /// <summary>
    /// Merge multiple PDF files into a single PDF
    /// </summary>
    /// <param name="pdfPaths">Ordered list of PDF file paths to merge</param>
    /// <param name="outputPath">Path where the merged PDF will be saved</param>
    /// <returns>True if successful</returns>
    Task<bool> MergePdfsAsync(List<string> pdfPaths, string outputPath);

    /// <summary>
    /// Get the number of pages in a PDF
    /// </summary>
    /// <param name="pdfPath">Path to the PDF file</param>
    /// <returns>Number of pages</returns>
    Task<int> GetPageCountAsync(string pdfPath);

    /// <summary>
    /// Get keywords from a PDF
    /// </summary>
    /// <param name="pdfPath">Path to the PDF file</param>
    /// <returns>List of keywords in order</returns>
    Task<List<string>> GetKeywordsAsync(string pdfPath);

    /// <summary>
    /// Get bookmarks from a PDF
    /// </summary>
    /// <param name="pdfPath">Path to the PDF file</param>
    /// <returns>List of bookmarks with their page numbers</returns>
    Task<List<BookmarkDto>> GetBookmarksAsync(string pdfPath);

    /// <summary>
    /// Validate if a file is a valid PDF
    /// </summary>
    /// <param name="pdfPath">Path to the file</param>
    /// <returns>True if valid PDF</returns>
    Task<bool> IsValidPdfAsync(string pdfPath);
}
