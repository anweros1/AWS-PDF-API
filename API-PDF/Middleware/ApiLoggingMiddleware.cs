using API_PDF.Models.Entities;
using API_PDF.Repositories.Interfaces;
using System.Diagnostics;
using System.Text;

namespace API_PDF.Middleware;

/// <summary>
/// Middleware to automatically log all API calls to the database
/// </summary>
public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiLoggingMiddleware> _logger;

    public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ILogRepository logRepository)
    {
        // Only log API endpoints (skip static files, health checks, etc.)
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        // Capture request body
        string requestBody = await ReadRequestBodyAsync(context.Request);

        // Create a memory stream to capture response
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        Exception? exception = null;
        int statusCode = 200;

        try
        {
            await _next(context);
            statusCode = context.Response.StatusCode;
        }
        catch (Exception ex)
        {
            exception = ex;
            statusCode = 500;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Capture response body
            string responseBodyText = await ReadResponseBodyAsync(responseBody);

            // Copy response back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);

            // Extract PDF GUID from route or request body
            string pdfGuid = ExtractPdfGuid(context, requestBody);

            // Extract application name from headers or default
            string applicationName = context.Request.Headers["X-Application-Name"].FirstOrDefault() ?? "Unknown";

            // Extract username from headers
            string? username = context.Request.Headers["X-Username"].FirstOrDefault();

            // Log to database (fire and forget to not block response)
            _ = Task.Run(async () =>
            {
                try
                {
                    var log = new ApiCallLog
                    {
                        PdfGuid = pdfGuid,
                        ApplicationName = applicationName,
                        Username = username,
                        Endpoint = context.Request.Path,
                        HttpMethod = context.Request.Method,
                        RequestBody = requestBody,
                        ResponseStatusCode = statusCode,
                        ResponseBody = responseBodyText,
                        ErrorMessage = exception?.Message,
                        DurationMs = stopwatch.ElapsedMilliseconds,
                        Timestamp = DateTime.UtcNow,
                        ClientIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                        IsSuccess = statusCode >= 200 && statusCode < 300
                    };

                    await logRepository.AddLogAsync(log);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to log API call to database");
                }
            });
        }
    }

    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        try
        {
            request.EnableBuffering();
            
            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            // Truncate if too large (max 10KB for logging)
            return body.Length > 10240 ? body.Substring(0, 10240) + "... [truncated]" : body;
        }
        catch
        {
            return string.Empty;
        }
    }

    private async Task<string> ReadResponseBodyAsync(MemoryStream responseBody)
    {
        try
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            // Truncate if too large (max 10KB for logging)
            return text.Length > 10240 ? text.Substring(0, 10240) + "... [truncated]" : text;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractPdfGuid(HttpContext context, string requestBody)
    {
        // Try to extract from route parameters
        if (context.Request.RouteValues.TryGetValue("guid", out var guidValue))
        {
            return guidValue?.ToString() ?? "unknown";
        }

        if (context.Request.RouteValues.TryGetValue("pdfGuid", out var pdfGuidValue))
        {
            return pdfGuidValue?.ToString() ?? "unknown";
        }

        // Try to extract from query string
        if (context.Request.Query.TryGetValue("guid", out var queryGuid))
        {
            return queryGuid.FirstOrDefault() ?? "unknown";
        }

        // Try to extract from request body (simple JSON parsing)
        if (!string.IsNullOrWhiteSpace(requestBody))
        {
            // Look for "pdfGuid": "value" pattern
            var match = System.Text.RegularExpressions.Regex.Match(
                requestBody, 
                @"""pdfGuid""\s*:\s*""([^""]+)""", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }

        return "unknown";
    }
}
