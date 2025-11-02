using API_PDF.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PDF.Controllers;

/// <summary>
/// Controller for S3 storage management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class S3Controller : ControllerBase
{
    private readonly IS3Service _s3Service;
    private readonly ILogger<S3Controller> _logger;

    public S3Controller(IS3Service s3Service, ILogger<S3Controller> logger)
    {
        _s3Service = s3Service;
        _logger = logger;
    }

    /// <summary>
    /// Check S3 connection and availability
    /// </summary>
    /// <returns>Health status of S3 service</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(S3HealthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckHealth()
    {
        try
        {
            var isAvailable = await _s3Service.IsS3AvailableAsync();
            
            var response = new S3HealthResponse
            {
                IsAvailable = isAvailable,
                Status = isAvailable ? "Connected" : "Unavailable",
                Message = isAvailable 
                    ? "S3 service is available and connected" 
                    : "S3 service is unavailable, using local fallback",
                Timestamp = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check S3 health");
            return Ok(new S3HealthResponse
            {
                IsAvailable = false,
                Status = "Error",
                Message = $"Health check failed: {ex.Message}",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Check if a PDF file exists in S3 or local storage
    /// </summary>
    /// <param name="guid">PDF GUID</param>
    /// <returns>File existence status</returns>
    [HttpGet("exists/{guid}")]
    [ProducesResponseType(typeof(FileExistsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckFileExists(string guid)
    {
        try
        {
            var exists = await _s3Service.PdfExistsAsync(guid);
            string? url = null;
            
            if (exists)
            {
                var (fileUrl, _) = await _s3Service.GetPdfUrlAsync(guid);
                url = fileUrl;
            }

            var response = new FileExistsResponse
            {
                PdfGuid = guid,
                Exists = exists,
                Url = url,
                Message = exists ? "File found" : "File not found"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check file existence for GUID: {Guid}", guid);
            return StatusCode(500, new { Message = "Failed to check file existence", Error = ex.Message });
        }
    }

    /// <summary>
    /// Download a PDF file from S3 or local storage
    /// </summary>
    /// <param name="guid">PDF GUID</param>
    /// <returns>PDF file stream</returns>
    [HttpGet("download/{guid}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFile(string guid)
    {
        try
        {
            // Check if file exists
            if (!await _s3Service.PdfExistsAsync(guid))
            {
                _logger.LogWarning("Download requested for non-existent PDF: {Guid}", guid);
                return NotFound(new { Message = $"PDF with GUID {guid} not found" });
            }

            // Get the file URL
            var (url, isStoredInS3) = await _s3Service.GetPdfUrlAsync(guid);
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogError("Failed to get URL for PDF: {Guid}", guid);
                return NotFound(new { Message = "Failed to get file URL" });
            }

            // If it's a local file path, read and return it
            if (!isStoredInS3)
            {
                if (!System.IO.File.Exists(url))
                {
                    _logger.LogError("Local file not found: {Path}", url);
                    return NotFound(new { Message = "File not found on disk" });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(url);
                return File(fileBytes, "application/pdf", $"{guid}.pdf");
            }

            // For S3 URLs, download the file and return it instead of redirecting
            // This prevents CORS issues and handles non-existent files properly
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                var response = await httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("S3 file not accessible: {Guid}, Status: {Status}", guid, response.StatusCode);
                    return NotFound(new { Message = $"PDF file not accessible in S3 (Status: {response.StatusCode})" });
                }
                
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                return File(fileBytes, "application/pdf", $"{guid}.pdf");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to download from S3: {Guid}", guid);
                return NotFound(new { Message = "Failed to download file from S3", Error = ex.Message });
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "S3 download timeout: {Guid}", guid);
                return StatusCode(504, new { Message = "Download timeout" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file for GUID: {Guid}", guid);
            return StatusCode(500, new { Message = "Failed to download file", Error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a PDF file from S3 or local storage
    /// </summary>
    /// <param name="guid">PDF GUID</param>
    /// <returns>Deletion status</returns>
    [HttpDelete("{guid}")]
    [ProducesResponseType(typeof(DeleteFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFile(string guid)
    {
        try
        {
            // Check if file exists
            if (!await _s3Service.PdfExistsAsync(guid))
            {
                return NotFound(new { Message = $"PDF with GUID {guid} not found" });
            }

            // Delete the file
            var success = await _s3Service.DeletePdfAsync(guid);

            if (success)
            {
                return Ok(new DeleteFileResponse
                {
                    PdfGuid = guid,
                    Success = true,
                    Message = "File deleted successfully"
                });
            }
            else
            {
                return StatusCode(500, new DeleteFileResponse
                {
                    PdfGuid = guid,
                    Success = false,
                    Message = "Failed to delete file"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file for GUID: {Guid}", guid);
            return StatusCode(500, new { Message = "Failed to delete file", Error = ex.Message });
        }
    }
}

/// <summary>
/// Response for S3 health check
/// </summary>
public class S3HealthResponse
{
    public bool IsAvailable { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Response for file existence check
/// </summary>
public class FileExistsResponse
{
    public string PdfGuid { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public string? Url { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Response for file deletion
/// </summary>
public class DeleteFileResponse
{
    public string PdfGuid { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
