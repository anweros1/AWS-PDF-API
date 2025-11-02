using API_PDF.Controllers;
using API_PDF.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace API_PDF.Tests.Controllers.Tests;

[TestFixture]
public class S3ControllerTests
{
    private Mock<IS3Service> _mockS3Service;
    private Mock<ILogger<S3Controller>> _mockLogger;
    private S3Controller _controller;

    [SetUp]
    public void Setup()
    {
        _mockS3Service = new Mock<IS3Service>();
        _mockLogger = new Mock<ILogger<S3Controller>>();
        _controller = new S3Controller(_mockS3Service.Object, _mockLogger.Object);
    }

    #region Health Check Tests

    [Test]
    public async Task CheckHealth_WhenS3Available_ShouldReturnConnectedStatus()
    {
        // Arrange
        _mockS3Service.Setup(s => s.IsS3AvailableAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CheckHealth();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as S3HealthResponse;
        
        response.Should().NotBeNull();
        response!.IsAvailable.Should().BeTrue();
        response.Status.Should().Be("Connected");
        response.Message.Should().Contain("available");
    }

    [Test]
    public async Task CheckHealth_WhenS3Unavailable_ShouldReturnUnavailableStatus()
    {
        // Arrange
        _mockS3Service.Setup(s => s.IsS3AvailableAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CheckHealth();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as S3HealthResponse;
        
        response.Should().NotBeNull();
        response!.IsAvailable.Should().BeFalse();
        response.Status.Should().Be("Unavailable");
        response.Message.Should().Contain("fallback");
    }

    [Test]
    public async Task CheckHealth_WhenExceptionThrown_ShouldReturnErrorStatus()
    {
        // Arrange
        _mockS3Service.Setup(s => s.IsS3AvailableAsync())
            .ThrowsAsync(new Exception("Connection failed"));

        // Act
        var result = await _controller.CheckHealth();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as S3HealthResponse;
        
        response.Should().NotBeNull();
        response!.IsAvailable.Should().BeFalse();
        response.Status.Should().Be("Error");
        response.Message.Should().Contain("failed");
    }

    #endregion

    #region File Exists Tests

    [Test]
    public async Task CheckFileExists_WhenFileExists_ShouldReturnTrueWithUrl()
    {
        // Arrange
        var guid = "test-guid-123";
        var expectedUrl = "C:\\PDFs\\Fallback\\test-guid-123.pdf";
        
        _mockS3Service.Setup(s => s.PdfExistsAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockS3Service.Setup(s => s.GetPdfUrlAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedUrl, false));

        // Act
        var result = await _controller.CheckFileExists(guid);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as FileExistsResponse;
        
        response.Should().NotBeNull();
        response!.PdfGuid.Should().Be(guid);
        response.Exists.Should().BeTrue();
        response.Url.Should().Be(expectedUrl);
        response.Message.Should().Contain("found");
    }

    [Test]
    public async Task CheckFileExists_WhenFileDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var guid = "non-existent-guid";
        
        _mockS3Service.Setup(s => s.PdfExistsAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CheckFileExists(guid);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as FileExistsResponse;
        
        response.Should().NotBeNull();
        response!.PdfGuid.Should().Be(guid);
        response.Exists.Should().BeFalse();
        response.Url.Should().BeNull();
        response.Message.Should().Contain("not found");
    }

    [Test]
    public async Task CheckFileExists_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var guid = "error-guid";
        
        _mockS3Service.Setup(s => s.PdfExistsAsync(guid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CheckFileExists(guid);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region Download Tests

    [Test]
    public async Task DownloadFile_WhenFileDoesNotExist_ShouldReturn404()
    {
        // Arrange
        var guid = "non-existent-guid";
        
        _mockS3Service.Setup(s => s.PdfExistsAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DownloadFile(guid);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task DownloadFile_WhenUrlIsNull_ShouldReturn404()
    {
        // Arrange
        var guid = "test-guid";
        
        _mockS3Service.Setup(s => s.PdfExistsAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockS3Service.Setup(s => s.GetPdfUrlAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((string?)null, false));

        // Act
        var result = await _controller.DownloadFile(guid);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task DownloadFile_WhenS3File_ShouldRedirect()
    {
        // Arrange
        var guid = "test-guid";
        var s3Url = "https://bucket.s3.amazonaws.com/test-guid.pdf";
        
        _mockS3Service.Setup(s => s.PdfExistsAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockS3Service.Setup(s => s.GetPdfUrlAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((s3Url, true));

        // Act
        var result = await _controller.DownloadFile(guid);

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult!.Url.Should().Be(s3Url);
    }

    [Test]
    public async Task DownloadFile_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var guid = "error-guid";
        
        _mockS3Service.Setup(s => s.PdfExistsAsync(guid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Download error"));

        // Act
        var result = await _controller.DownloadFile(guid);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task DeleteFile_WhenFileDoesNotExist_ShouldReturn404()
    {
        // Arrange
        var guid = "non-existent-guid";
        
        _mockS3Service.Setup(s => s.PdfExistsAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteFile(guid);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task DeleteFile_WhenDeleteSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        var guid = "test-guid";
        
        _mockS3Service.Setup(s => s.PdfExistsAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockS3Service.Setup(s => s.DeletePdfAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteFile(guid);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as DeleteFileResponse;
        
        response.Should().NotBeNull();
        response!.PdfGuid.Should().Be(guid);
        response.Success.Should().BeTrue();
        response.Message.Should().Contain("deleted successfully");
    }

    [Test]
    public async Task DeleteFile_WhenDeleteFails_ShouldReturn500()
    {
        // Arrange
        var guid = "test-guid";
        
        _mockS3Service.Setup(s => s.PdfExistsAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockS3Service.Setup(s => s.DeletePdfAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteFile(guid);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        
        var response = objectResult.Value as DeleteFileResponse;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
    }

    [Test]
    public async Task DeleteFile_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var guid = "error-guid";
        
        _mockS3Service.Setup(s => s.PdfExistsAsync(guid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Delete error"));

        // Act
        var result = await _controller.DeleteFile(guid);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion
}
