using API_PDF.Models.DTOs;
using API_PDF.Services.Interfaces;
using Microsoft.Extensions.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.AcroForms;
using System.Text;

namespace API_PDF.Services;

/// <summary>
/// Service for PDF manipulation operations using PdfSharp
/// </summary>
public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> AddBookmarksAsync(string pdfPath, List<BookmarkDto> bookmarks, string outputPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(pdfPath))
                {
                    _logger.LogWarning("PDF file not found: {PdfPath}", pdfPath);
                    return false;
                }

                // Open PDF for modification
                var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                var outputDoc = new PdfDocument();
                
                // Copy all pages
                foreach (PdfPage page in document.Pages)
                {
                    outputDoc.AddPage(page);
                }

                foreach (var bookmark in bookmarks)
                {
                    try
                    {
                        // Validate page number
                        if (bookmark.PageNumber < 1 || bookmark.PageNumber > outputDoc.PageCount)
                        {
                            _logger.LogWarning("Invalid page number {PageNumber} for bookmark '{Value}'. Skipping.", 
                                bookmark.PageNumber, bookmark.Value);
                            continue;
                        }

                        // Add bookmark (PdfSharp uses 0-based index)
                        var page = outputDoc.Pages[bookmark.PageNumber - 1];
                        outputDoc.Outlines.Add(bookmark.Value, page, true);
                        
                        _logger.LogDebug("Added bookmark '{Value}' at page {PageNumber}", 
                            bookmark.Value, bookmark.PageNumber);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to add bookmark '{Value}'. Skipping.", bookmark.Value);
                    }
                }

                // Save to output path
                outputDoc.Save(outputPath);
                outputDoc.Dispose();
                document.Dispose();

                _logger.LogInformation("Added {Count} bookmarks to PDF: {OutputPath}", bookmarks.Count, outputPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add bookmarks to PDF: {PdfPath}", pdfPath);
                return false;
            }
        });
    }

    public async Task<bool> AssignVariablesAsync(string pdfPath, Dictionary<string, string> variables, string outputPath)
    {
        return await Task.Run(() =>
        {
            PdfDocument? document = null;
            
            try
            {
                if (!File.Exists(pdfPath))
                {
                    _logger.LogWarning("PDF file not found: {PdfPath}", pdfPath);
                    return false;
                }

                // Open document for modification (not import)
                document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Modify);
                
                // Check if document has AcroForm
                var acroForm = document.AcroForm;
                if (acroForm == null)
                {
                    _logger.LogWarning("PDF has no AcroForm (no form fields): {PdfPath}", pdfPath);
                    // Still save a copy
                    document.Save(outputPath);
                    return true;
                }
                
                var fields = acroForm.Fields;
                if (fields == null || fields.Count == 0)
                {
                    _logger.LogWarning("PDF has no form fields: {PdfPath}", pdfPath);
                    // Still save a copy
                    document.Save(outputPath);
                    return true;
                }

                int assignedCount = 0;
                
                _logger.LogInformation("Found {Count} form fields in PDF", fields.Count);
                
                foreach (var variable in variables)
                {
                    try
                    {
                        // Find field by name
                        PdfAcroField? field = null;
                        
                        // Try exact match first
                        if (fields.Names.Contains(variable.Key))
                        {
                            field = fields[variable.Key];
                        }
                        
                        if (field != null)
                        {
                            // Set the value based on field type
                            if (field is PdfTextField textField)
                            {
                                textField.Value = new PdfString(variable.Value);
                                assignedCount++;
                                _logger.LogDebug("Assigned text field '{FieldName}': {Value}", variable.Key, variable.Value);
                            }
                            else if (field is PdfCheckBoxField checkBox)
                            {
                                // Handle checkbox values
                                bool isChecked = variable.Value.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
                                               variable.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                               variable.Value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                                               variable.Value.Equals("On", StringComparison.OrdinalIgnoreCase);
                                
                                checkBox.Checked = isChecked;
                                assignedCount++;
                                _logger.LogDebug("Assigned checkbox '{FieldName}': {Value}", variable.Key, isChecked);
                            }
                            else
                            {
                                // For other field types, try setting the value directly
                                if (field.Elements.ContainsKey("/V"))
                                {
                                    field.Elements.SetString("/V", variable.Value);
                                    assignedCount++;
                                    _logger.LogDebug("Assigned generic field '{FieldName}': {Value}", variable.Key, variable.Value);
                                }
                                else
                                {
                                    _logger.LogWarning("Field '{FieldName}' has no value element", variable.Key);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Field '{FieldName}' not found in PDF", variable.Key);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to assign value to field '{FieldName}'. Skipping.", variable.Key);
                    }
                }
                
                _logger.LogInformation("Successfully assigned {Count} of {Total} variables to PDF", 
                    assignedCount, variables.Count);

                // Save the modified document
                document.Save(outputPath);
                document.Dispose();
                document = null;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign variables to PDF: {PdfPath}", pdfPath);
                return false;
            }
            finally
            {
                document?.Dispose();
            }
        });
    }

    public async Task<bool> AddKeywordsAsync(string pdfPath, List<string> keywords, string outputPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(pdfPath))
                {
                    _logger.LogWarning("PDF file not found: {PdfPath}", pdfPath);
                    return false;
                }

                var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Modify);
                
                // Join keywords with comma to preserve order
                var keywordsString = string.Join(", ", keywords);
                document.Info.Keywords = keywordsString;

                document.Save(outputPath);
                document.Dispose();

                _logger.LogInformation("Added {Count} keywords to PDF: {OutputPath}", keywords.Count, outputPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add keywords to PDF: {PdfPath}", pdfPath);
                return false;
            }
        });
    }

    public async Task<bool> MergePdfsAsync(List<string> pdfPaths, string outputPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (pdfPaths == null || pdfPaths.Count == 0)
                {
                    _logger.LogWarning("No PDF files provided for merging");
                    return false;
                }

                // Validate all files exist
                foreach (var pdfPath in pdfPaths)
                {
                    if (!File.Exists(pdfPath))
                    {
                        _logger.LogWarning("PDF file not found: {PdfPath}", pdfPath);
                        return false;
                    }
                }

                // Create output document
                var outputDocument = new PdfDocument();

                foreach (var pdfPath in pdfPaths)
                {
                    try
                    {
                        // Open source document
                        var inputDocument = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                        
                        // Copy all pages from source to output
                        foreach (PdfPage page in inputDocument.Pages)
                        {
                            outputDocument.AddPage(page);
                        }
                        
                        _logger.LogDebug("Merged PDF: {PdfPath} ({PageCount} pages)", 
                            pdfPath, inputDocument.PageCount);
                        
                        inputDocument.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to merge PDF: {PdfPath}", pdfPath);
                        outputDocument.Dispose();
                        return false;
                    }
                }

                // Save merged document
                outputDocument.Save(outputPath);
                outputDocument.Dispose();

                _logger.LogInformation("Merged {Count} PDFs into: {OutputPath}", pdfPaths.Count, outputPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to merge PDFs");
                return false;
            }
        });
    }

    public async Task<int> GetPageCountAsync(string pdfPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(pdfPath))
                {
                    _logger.LogWarning("PDF file not found: {PdfPath}", pdfPath);
                    return 0;
                }

                var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                var pageCount = document.PageCount;
                document.Dispose();
                
                return pageCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get page count from PDF: {PdfPath}", pdfPath);
                return 0;
            }
        });
    }

    public async Task<List<string>> GetKeywordsAsync(string pdfPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(pdfPath))
                {
                    _logger.LogWarning("PDF file not found: {PdfPath}", pdfPath);
                    return new List<string>();
                }

                var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                var keywordsString = document.Info.Keywords;
                document.Dispose();
                
                if (string.IsNullOrWhiteSpace(keywordsString))
                {
                    return new List<string>();
                }

                // Split by comma and trim whitespace
                return keywordsString
                    .Split(',')
                    .Select(k => k.Trim())
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get keywords from PDF: {PdfPath}", pdfPath);
                return new List<string>();
            }
        });
    }

    public async Task<List<BookmarkDto>> GetBookmarksAsync(string pdfPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(pdfPath))
                {
                    _logger.LogWarning("PDF file not found: {PdfPath}", pdfPath);
                    return new List<BookmarkDto>();
                }

                var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                var bookmarks = new List<BookmarkDto>();
                
                // Extract bookmarks from outlines
                ExtractBookmarksRecursive(document.Outlines, bookmarks, document);

                document.Dispose();

                _logger.LogDebug("Extracted {Count} bookmarks from PDF: {PdfPath}", bookmarks.Count, pdfPath);
                return bookmarks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get bookmarks from PDF: {PdfPath}", pdfPath);
                return new List<BookmarkDto>();
            }
        });
    }

    private void ExtractBookmarksRecursive(PdfOutlineCollection outlines, List<BookmarkDto> bookmarks, PdfDocument document)
    {
        if (outlines == null || outlines.Count == 0) return;

        foreach (PdfOutline outline in outlines)
        {
            try
            {
                var title = outline.Title;
                if (!string.IsNullOrWhiteSpace(title))
                {
                    // Try to get page number
                    int pageNumber = 1; // default
                    
                    try
                    {
                        if (outline.DestinationPage != null)
                        {
                            // Find page index
                            for (int i = 0; i < document.Pages.Count; i++)
                            {
                                if (document.Pages[i] == outline.DestinationPage)
                                {
                                    pageNumber = i + 1; // Convert to 1-based
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Use default page number if extraction fails
                    }
                    
                    bookmarks.Add(new BookmarkDto
                    {
                        Value = title,
                        PageNumber = pageNumber
                    });
                }

                // Recursively process children
                if (outline.Outlines != null && outline.Outlines.Count > 0)
                {
                    ExtractBookmarksRecursive(outline.Outlines, bookmarks, document);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract bookmark");
            }
        }
    }

    public async Task<bool> IsValidPdfAsync(string pdfPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(pdfPath))
                {
                    return false;
                }

                // Try to open the PDF
                var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                
                // If we can open it and get page count, it's valid
                _ = document.PageCount;
                document.Dispose();
                
                return true;
            }
            catch
            {
                return false;
            }
        });
    }
}
