using API_PDF.Models;
using API_PDF.Models.DTOs;
using API_PDF.Models.Entities;
using API_PDF.Repositories.Interfaces;
using API_PDF.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace API_PDF.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    private readonly IPdfService _pdfService;
    private readonly IS3Service _s3Service;
    private readonly IPdfHistoryRepository _historyRepository;
    private readonly ILogger<PdfController> _logger;
    private readonly PdfSettings _pdfSettings;
    private readonly IHttpClientFactory _httpClientFactory;

    public PdfController(
        IPdfService pdfService,
        IS3Service s3Service,
        IPdfHistoryRepository historyRepository,
        ILogger<PdfController> logger,
        IOptions<PdfSettings> pdfSettings,
        IHttpClientFactory httpClientFactory)
    {
        _pdfService = pdfService;
        _s3Service = s3Service;
        _historyRepository = historyRepository;
        _logger = logger;
        _pdfSettings = pdfSettings.Value;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Upload a new PDF file directly (multipart/form-data)
    /// </summary>
    [HttpPost("upload-from-file")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AddPdfResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPdfFromFile(
        IFormFile file,
        [FromForm] string applicationName)
    {
        string? tempFilePath = null;
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "No file provided" });
            }

            // Validate file size
            var maxSizeBytes = _pdfSettings.MaxFileSizeMb * 1024 * 1024;
            if (file.Length > maxSizeBytes)
            {
                return BadRequest(new { Message = $"File size ({file.Length / 1024 / 1024}MB) exceeds maximum allowed size ({_pdfSettings.MaxFileSizeMb}MB)" });
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".pdf")
            {
                return BadRequest(new { Message = "Only PDF files are allowed" });
            }

            var pdfGuid = Guid.NewGuid().ToString();
            var tempFolder = _pdfSettings.TempFolder;
            
            // Ensure temp folder exists
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            
            tempFilePath = Path.Combine(tempFolder, $"{pdfGuid}.pdf");

            // Save uploaded file to temp location
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Validate it's a PDF
            if (!await _pdfService.IsValidPdfAsync(tempFilePath))
            {
                return BadRequest(new { Message = "Invalid PDF file" });
            }

            // Get page count
            var pageCount = await _pdfService.GetPageCountAsync(tempFilePath);

            // Upload to S3 or local fallback
            var (s3Url, isStoredInS3) = await _s3Service.UploadPdfAsync(tempFilePath, pdfGuid);

            // Create history record
            var history = new PdfHistory
            {
                PdfGuid = pdfGuid,
                ApplicationName = applicationName,
                OriginalUrl = file.FileName, // Store filename in OriginalUrl for file uploads
                PageCount = pageCount,
                S3Url = s3Url,
                IsStoredInS3 = isStoredInS3,
                FileSizeBytes = file.Length,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _historyRepository.AddAsync(history);

            var response = new AddPdfResponse
            {
                PdfGuid = pdfGuid,
                S3Url = s3Url,
                IsStoredInS3 = isStoredInS3,
                Success = true,
                Message = "PDF uploaded successfully",
                History = MapToHistoryDto(history)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload PDF file");
            return StatusCode(500, new { Message = "Failed to upload PDF", Error = ex.Message });
        }
        finally
        {
            // Always clean up temp file
            if (tempFilePath != null && System.IO.File.Exists(tempFilePath))
            {
                try
                {
                    System.IO.File.Delete(tempFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temp file: {Path}", tempFilePath);
                }
            }
        }
    }

    /// <summary>
    /// Upload a new PDF from URL
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(AddPdfResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPdf([FromBody] AddPdfRequest request)
    {
        string? tempFilePath = null;
        try
        {
            var pdfGuid = Guid.NewGuid().ToString();
            var tempFolder = _pdfSettings.TempFolder;
            
            // Ensure temp folder exists
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            
            tempFilePath = Path.Combine(tempFolder, $"{pdfGuid}.pdf");

            // Download PDF from provided URL with size validation
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            // Check file size before downloading
            using var headResponse = await httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, request.FileUrl));
            headResponse.EnsureSuccessStatusCode();
            
            var contentLength = headResponse.Content.Headers.ContentLength;
            var maxSizeBytes = _pdfSettings.MaxFileSizeMb * 1024 * 1024;
            
            if (contentLength.HasValue && contentLength.Value > maxSizeBytes)
            {
                return BadRequest(new { Message = $"File size ({contentLength.Value / 1024 / 1024}MB) exceeds maximum allowed size ({_pdfSettings.MaxFileSizeMb}MB)" });
            }
            
            var pdfBytes = await httpClient.GetByteArrayAsync(request.FileUrl);
            
            // Double-check size after download
            if (pdfBytes.Length > maxSizeBytes)
            {
                return BadRequest(new { Message = $"File size exceeds maximum allowed size ({_pdfSettings.MaxFileSizeMb}MB)" });
            }
            
            await System.IO.File.WriteAllBytesAsync(tempFilePath, pdfBytes);

            // Validate it's a PDF
            if (!await _pdfService.IsValidPdfAsync(tempFilePath))
            {
                return BadRequest(new { Message = "Invalid PDF file" });
            }

            // Get page count
            var pageCount = await _pdfService.GetPageCountAsync(tempFilePath);

            // Upload to S3 or local fallback
            var (s3Url, isStoredInS3) = await _s3Service.UploadPdfAsync(tempFilePath, pdfGuid);

            // Create history record
            var history = new PdfHistory
            {
                PdfGuid = pdfGuid,
                ApplicationName = request.ApplicationName,
                OriginalUrl = request.FileUrl,
                S3Url = isStoredInS3 ? s3Url : null,
                S3BucketName = isStoredInS3 ? "configured-bucket" : null,
                LocalFilePath = !isStoredInS3 ? s3Url : null,
                IsStoredInS3 = isStoredInS3,
                FileSizeBytes = new FileInfo(tempFilePath).Length,
                PageCount = pageCount,
                IsMerged = false,
                IsDeleted = false
            };

            await _historyRepository.AddAsync(history);

            var response = new AddPdfResponse
            {
                PdfGuid = pdfGuid,
                S3Url = s3Url,
                IsStoredInS3 = isStoredInS3,
                Success = true,
                Message = "PDF uploaded successfully",
                History = MapToHistoryDto(history)
            };

            return Ok(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to download PDF from URL: {Url}", request.FileUrl);
            return BadRequest(new { Message = "Failed to download PDF from provided URL", Error = ex.Message });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "PDF download timed out: {Url}", request.FileUrl);
            return BadRequest(new { Message = "PDF download timed out" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload PDF");
            return StatusCode(500, new { Message = "Failed to upload PDF", Error = ex.Message });
        }
        finally
        {
            // Always clean up temp file
            if (tempFilePath != null && System.IO.File.Exists(tempFilePath))
            {
                try
                {
                    System.IO.File.Delete(tempFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temp file: {Path}", tempFilePath);
                }
            }
        }
    }

    /// <summary>
    /// Add bookmarks to a PDF
    /// </summary>
    [HttpPost("{guid}/bookmarks")]
    [ProducesResponseType(typeof(PdfOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddBookmarks(string guid, [FromBody] AddBookmarksRequest request)
    {
        try
        {
            request.PdfGuid = guid;

            var history = await _historyRepository.GetByGuidAsync(guid);
            if (history == null)
            {
                return NotFound(new { Message = $"PDF with GUID {guid} not found" });
            }

            var tempFolder = _pdfSettings.TempFolder;
            
            // Ensure temp folder exists
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            
            var inputPath = Path.Combine(tempFolder, $"{guid}_input.pdf");
            var outputPath = Path.Combine(tempFolder, $"{guid}_output.pdf");

            // Download PDF
            if (!await _s3Service.DownloadPdfAsync(guid, inputPath))
            {
                return NotFound(new { Message = "Failed to download PDF" });
            }

            // Add bookmarks
            var success = await _pdfService.AddBookmarksAsync(inputPath, request.Bookmarks, outputPath);
            if (!success)
            {
                return BadRequest(new { Message = "Failed to add bookmarks" });
            }

            // Upload modified PDF
            var (s3Url, isStoredInS3) = await _s3Service.UploadPdfAsync(outputPath, guid);

            // Update history
            history.Bookmarks = JsonSerializer.Serialize(request.Bookmarks);
            await _historyRepository.UpdateAsync(history);

            // Clean up
            System.IO.File.Delete(inputPath);
            System.IO.File.Delete(outputPath);

            return Ok(new PdfOperationResponse
            {
                PdfGuid = guid,
                Success = true,
                Message = $"Added {request.Bookmarks.Count} bookmarks",
                S3Url = s3Url,
                History = MapToHistoryDto(history)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add bookmarks to PDF {Guid}", guid);
            return StatusCode(500, new { Message = "Failed to add bookmarks", Error = ex.Message });
        }
    }

    /// <summary>
    /// Assign variables to PDF form fields
    /// </summary>
    [HttpPost("{guid}/variables")]
    [ProducesResponseType(typeof(PdfOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignVariables(string guid, [FromBody] AssignVariablesRequest request)
    {
        try
        {
            request.PdfGuid = guid;

            var history = await _historyRepository.GetByGuidAsync(guid);
            if (history == null)
            {
                return NotFound(new { Message = $"PDF with GUID {guid} not found" });
            }

            var tempFolder = _pdfSettings.TempFolder;
            
            // Ensure temp folder exists
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            
            var inputPath = Path.Combine(tempFolder, $"{guid}_input.pdf");
            var outputPath = Path.Combine(tempFolder, $"{guid}_output.pdf");

            if (!await _s3Service.DownloadPdfAsync(guid, inputPath))
            {
                return NotFound(new { Message = "Failed to download PDF" });
            }

            var success = await _pdfService.AssignVariablesAsync(inputPath, request.Variables, outputPath);
            if (!success)
            {
                return BadRequest(new { Message = "Failed to assign variables" });
            }

            var (s3Url, isStoredInS3) = await _s3Service.UploadPdfAsync(outputPath, guid);

            history.Variables = JsonSerializer.Serialize(request.Variables);
            await _historyRepository.UpdateAsync(history);

            System.IO.File.Delete(inputPath);
            System.IO.File.Delete(outputPath);

            return Ok(new PdfOperationResponse
            {
                PdfGuid = guid,
                Success = true,
                Message = $"Assigned {request.Variables.Count} variables",
                S3Url = s3Url,
                History = MapToHistoryDto(history)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign variables to PDF {Guid}", guid);
            return StatusCode(500, new { Message = "Failed to assign variables", Error = ex.Message });
        }
    }

    /// <summary>
    /// Add keywords to a PDF (order preserved)
    /// </summary>
    [HttpPost("{guid}/keywords")]
    [ProducesResponseType(typeof(PdfOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddKeywords(string guid, [FromBody] AddKeywordsRequest request)
    {
        try
        {
            request.PdfGuid = guid;

            var history = await _historyRepository.GetByGuidAsync(guid);
            if (history == null)
            {
                return NotFound(new { Message = $"PDF with GUID {guid} not found" });
            }

            var tempFolder = _pdfSettings.TempFolder;
            
            // Ensure temp folder exists
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            
            var inputPath = Path.Combine(tempFolder, $"{guid}_input.pdf");
            var outputPath = Path.Combine(tempFolder, $"{guid}_output.pdf");

            if (!await _s3Service.DownloadPdfAsync(guid, inputPath))
            {
                return NotFound(new { Message = "Failed to download PDF" });
            }

            var success = await _pdfService.AddKeywordsAsync(inputPath, request.Keywords, outputPath);
            if (!success)
            {
                return BadRequest(new { Message = "Failed to add keywords" });
            }

            var (s3Url, isStoredInS3) = await _s3Service.UploadPdfAsync(outputPath, guid);

            history.Keywords = JsonSerializer.Serialize(request.Keywords);
            await _historyRepository.UpdateAsync(history);

            System.IO.File.Delete(inputPath);
            System.IO.File.Delete(outputPath);

            return Ok(new PdfOperationResponse
            {
                PdfGuid = guid,
                Success = true,
                Message = $"Added {request.Keywords.Count} keywords (order preserved)",
                S3Url = s3Url,
                History = MapToHistoryDto(history)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add keywords to PDF {Guid}", guid);
            return StatusCode(500, new { Message = "Failed to add keywords", Error = ex.Message });
        }
    }

    /// <summary>
    /// Merge multiple PDFs into one (order preserved)
    /// </summary>
    [HttpPost("merge")]
    [ProducesResponseType(typeof(AddPdfResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MergePdfs([FromBody] MergePdfsRequest request)
    {
        try
        {
            var tempFolder = _pdfSettings.TempFolder;
            
            // Ensure temp folder exists
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            
            var pdfPaths = new List<string>();

            // Download all PDFs
            foreach (var guid in request.PdfGuids)
            {
                var path = Path.Combine(tempFolder, $"{guid}_merge.pdf");
                if (!await _s3Service.DownloadPdfAsync(guid, path))
                {
                    // Clean up downloaded files
                    foreach (var p in pdfPaths)
                {
                    if (System.IO.File.Exists(p))
                    {
                        System.IO.File.Delete(p);
                    }
                }
                    return NotFound(new { Message = $"PDF with GUID {guid} not found" });
                }
                pdfPaths.Add(path);
            }

            var mergedGuid = Guid.NewGuid().ToString();
            var mergedPath = Path.Combine(tempFolder, $"{mergedGuid}_merged.pdf");

            // Merge PDFs
            var success = await _pdfService.MergePdfsAsync(pdfPaths, mergedPath);
            if (!success)
            {
                foreach (var p in pdfPaths)
                {
                    if (System.IO.File.Exists(p))
                    {
                        System.IO.File.Delete(p);
                    }
                }
                return BadRequest(new { Message = "Failed to merge PDFs" });
            }

            // Upload merged PDF
            var (s3Url, isStoredInS3) = await _s3Service.UploadPdfAsync(mergedPath, mergedGuid);

            // Get page count
            var pageCount = await _pdfService.GetPageCountAsync(mergedPath);

            // Create history record
            var history = new PdfHistory
            {
                PdfGuid = mergedGuid,
                ApplicationName = request.ApplicationName,
                OriginalUrl = "merged",
                S3Url = isStoredInS3 ? s3Url : null,
                LocalFilePath = !isStoredInS3 ? s3Url : null,
                IsStoredInS3 = isStoredInS3,
                FileSizeBytes = new FileInfo(mergedPath).Length,
                PageCount = pageCount,
                IsMerged = true,
                SourcePdfGuids = JsonSerializer.Serialize(request.PdfGuids),
                IsDeleted = false
            };

            await _historyRepository.AddAsync(history);

            // Clean up
            foreach (var p in pdfPaths) System.IO.File.Delete(p);
            System.IO.File.Delete(mergedPath);

            return Ok(new AddPdfResponse
            {
                PdfGuid = mergedGuid,
                S3Url = s3Url,
                IsStoredInS3 = isStoredInS3,
                Success = true,
                Message = $"Merged {request.PdfGuids.Count} PDFs successfully",
                History = MapToHistoryDto(history)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to merge PDFs");
            return StatusCode(500, new { Message = "Failed to merge PDFs", Error = ex.Message });
        }
    }

    /// <summary>
    /// Get PDF history by GUID
    /// </summary>
    [HttpGet("{guid}")]
    [ProducesResponseType(typeof(PdfHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPdfHistory(string guid)
    {
        var history = await _historyRepository.GetByGuidAsync(guid);
        if (history == null)
        {
            return NotFound(new { Message = $"PDF with GUID {guid} not found" });
        }

        return Ok(MapToHistoryDto(history));
    }

    private PdfHistoryDto MapToHistoryDto(PdfHistory history)
    {
        return new PdfHistoryDto
        {
            PdfGuid = history.PdfGuid,
            ApplicationName = history.ApplicationName,
            OriginalUrl = history.OriginalUrl,
            S3Url = history.S3Url,
            IsStoredInS3 = history.IsStoredInS3,
            FileSizeBytes = history.FileSizeBytes,
            PageCount = history.PageCount,
            Keywords = string.IsNullOrWhiteSpace(history.Keywords) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(history.Keywords) ?? new List<string>(),
            Bookmarks = string.IsNullOrWhiteSpace(history.Bookmarks) 
                ? new List<BookmarkDto>() 
                : JsonSerializer.Deserialize<List<BookmarkDto>>(history.Bookmarks) ?? new List<BookmarkDto>(),
            Variables = string.IsNullOrWhiteSpace(history.Variables) 
                ? new Dictionary<string, string>() 
                : JsonSerializer.Deserialize<Dictionary<string, string>>(history.Variables) ?? new Dictionary<string, string>(),
            IsMerged = history.IsMerged,
            SourcePdfGuids = string.IsNullOrWhiteSpace(history.SourcePdfGuids) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(history.SourcePdfGuids) ?? new List<string>(),
            CreatedAt = history.CreatedAt,
            UpdatedAt = history.UpdatedAt
        };
    }
}
