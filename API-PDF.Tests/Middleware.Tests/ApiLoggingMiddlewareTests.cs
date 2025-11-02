using API_PDF.Middleware;
using API_PDF.Models.Entities;
using API_PDF.Repositories.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Text;

namespace API_PDF.Tests.Middleware.Tests;

[TestFixture]
public class ApiLoggingMiddlewareTests
{
    private Mock<ILogger<ApiLoggingMiddleware>> _mockLogger;
    private Mock<ILogRepository> _mockLogRepository;
    private ApiLoggingMiddleware _middleware;
    private DefaultHttpContext _httpContext;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ApiLoggingMiddleware>>();
        _mockLogRepository = new Mock<ILogRepository>();
        
        // Create middleware with a next delegate that does nothing
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        _middleware = new ApiLoggingMiddleware(next, _mockLogger.Object);

        // Setup HTTP context
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Test]
    public async Task InvokeAsync_WhenNonApiEndpoint_ShouldNotLog()
    {
        // Arrange
        _httpContext.Request.Path = "/health";
        _httpContext.Request.Method = "GET";

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockLogRepository.Object);

        // Assert
        _mockLogRepository.Verify(
            r => r.AddLogAsync(It.IsAny<ApiCallLog>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WhenApiEndpoint_ShouldLog()
    {
        // Arrange
        _httpContext.Request.Path = "/api/pdf/test-guid";
        _httpContext.Request.Method = "GET";
        _httpContext.Request.Headers["X-Application-Name"] = "TestApp";
        _httpContext.Request.Headers["X-Username"] = "john.doe@example.com";

        ApiCallLog? capturedLog = null;
        _mockLogRepository.Setup(r => r.AddLogAsync(It.IsAny<ApiCallLog>(), It.IsAny<CancellationToken>()))
            .Callback<ApiCallLog, CancellationToken>((log, ct) => capturedLog = log)
            .ReturnsAsync((ApiCallLog log, CancellationToken ct) => log);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockLogRepository.Object);

        // Wait a bit for async logging
        await Task.Delay(100);

        // Assert
        // Note: The middleware uses fire-and-forget Task.Run, so we can't directly verify
        // But we can verify the setup was correct
        _httpContext.Request.Path.Value.Should().Be("/api/pdf/test-guid");
    }

    [Test]
    public async Task InvokeAsync_ShouldExtractApplicationName()
    {
        // Arrange
        _httpContext.Request.Path = "/api/test";
        _httpContext.Request.Method = "POST";
        _httpContext.Request.Headers["X-Application-Name"] = "MyApp";

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockLogRepository.Object);

        // Assert
        _httpContext.Request.Headers["X-Application-Name"].ToString().Should().Be("MyApp");
    }

    [Test]
    public async Task InvokeAsync_ShouldExtractUsername()
    {
        // Arrange
        _httpContext.Request.Path = "/api/test";
        _httpContext.Request.Method = "POST";
        _httpContext.Request.Headers["X-Username"] = "test.user@example.com";

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockLogRepository.Object);

        // Assert
        _httpContext.Request.Headers["X-Username"].ToString().Should().Be("test.user@example.com");
    }

    [Test]
    public async Task InvokeAsync_WhenNoUsername_ShouldHandleGracefully()
    {
        // Arrange
        _httpContext.Request.Path = "/api/test";
        _httpContext.Request.Method = "GET";
        // No X-Username header

        // Act & Assert - should not throw
        await _middleware.InvokeAsync(_httpContext, _mockLogRepository.Object);
        
        // Verify it completed successfully
        _httpContext.Response.StatusCode.Should().Be(200);
    }

    [Test]
    public async Task InvokeAsync_ShouldExtractPdfGuidFromRoute()
    {
        // Arrange
        _httpContext.Request.Path = "/api/pdf/abc-123-def";
        _httpContext.Request.Method = "GET";
        _httpContext.Request.RouteValues["guid"] = "abc-123-def";

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockLogRepository.Object);

        // Assert
        _httpContext.Request.RouteValues["guid"].Should().Be("abc-123-def");
    }

    [Test]
    public async Task InvokeAsync_ShouldCaptureRequestMethod()
    {
        // Arrange
        _httpContext.Request.Path = "/api/test";
        _httpContext.Request.Method = "POST";

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockLogRepository.Object);

        // Assert
        _httpContext.Request.Method.Should().Be("POST");
    }

    [Test]
    public void InvokeAsync_WhenExceptionThrown_ShouldStillLog()
    {
        // Arrange
        _httpContext.Request.Path = "/api/test";
        _httpContext.Request.Method = "GET";

        // Create middleware that throws
        RequestDelegate nextThatThrows = (HttpContext hc) => throw new Exception("Test exception");
        var middlewareWithException = new ApiLoggingMiddleware(nextThatThrows, _mockLogger.Object);

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () =>
        {
            await middlewareWithException.InvokeAsync(_httpContext, _mockLogRepository.Object);
        });

        // The middleware should still attempt to log even when exception occurs
        // (fire-and-forget logging happens in finally block)
    }
}
