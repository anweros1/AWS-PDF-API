using API_PDF.Controllers;
using API_PDF.Models;
using API_PDF.Models.DTOs;
using API_PDF.Models.Entities;
using API_PDF.Repositories.Interfaces;
using API_PDF.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace API_PDF.Tests.Controllers.Tests;

[TestFixture]
public class PdfControllerTests
{
    private Mock<IPdfService> _mockPdfService;
    private Mock<IS3Service> _mockS3Service;
    private Mock<IPdfHistoryRepository> _mockHistoryRepository;
    private Mock<ILogger<PdfController>> _mockLogger;
    private Mock<IOptions<PdfSettings>> _mockPdfSettings;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private PdfController _controller;
    private PdfSettings _pdfSettings;

    [SetUp]
    public void Setup()
    {
        _mockPdfService = new Mock<IPdfService>();
        _mockS3Service = new Mock<IS3Service>();
        _mockHistoryRepository = new Mock<IPdfHistoryRepository>();
        _mockLogger = new Mock<ILogger<PdfController>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        
        _pdfSettings = new PdfSettings
        {
            TempFolder = "C:\\Temp\\PDFs",
            LocalFallbackFolder = "C:\\PDFs\\Fallback"
        };
        _mockPdfSettings = new Mock<IOptions<PdfSettings>>();
        _mockPdfSettings.Setup(x => x.Value).Returns(_pdfSettings);

        _controller = new PdfController(
            _mockPdfService.Object,
            _mockS3Service.Object,
            _mockHistoryRepository.Object,
            _mockLogger.Object,
            _mockPdfSettings.Object,
            _mockHttpClientFactory.Object);
    }

    #region GetPdfHistory Tests

    [Test]
    public async Task GetPdfHistory_WhenPdfExists_ShouldReturnHistory()
    {
        // Arrange
        var guid = "test-guid-123";
        var history = new PdfHistory
        {
            PdfGuid = guid,
            ApplicationName = "TestApp",
            PageCount = 5
        };

        _mockHistoryRepository.Setup(r => r.GetByGuidAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetPdfHistory(guid);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as PdfHistoryDto;
        response.Should().NotBeNull();
        response!.PdfGuid.Should().Be(guid);
    }

    [Test]
    public async Task GetPdfHistory_WhenPdfNotFound_ShouldReturn404()
    {
        // Arrange
        var guid = "non-existent-guid";
        _mockHistoryRepository.Setup(r => r.GetByGuidAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PdfHistory?)null);

        // Act
        var result = await _controller.GetPdfHistory(guid);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region AddBookmarks Tests

    [Test]
    public async Task AddBookmarks_WhenPdfNotFound_ShouldReturn404()
    {
        // Arrange
        var guid = "non-existent-guid";
        var request = new AddBookmarksRequest
        {
            Bookmarks = new List<BookmarkDto>
            {
                new BookmarkDto { Value = "Chapter 1", PageNumber = 1 }
            }
        };

        _mockHistoryRepository.Setup(r => r.GetByGuidAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PdfHistory?)null);

        // Act
        var result = await _controller.AddBookmarks(guid, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task AddBookmarks_WhenDownloadFails_ShouldReturn404()
    {
        // Arrange
        var guid = "test-guid";
        var request = new AddBookmarksRequest
        {
            Bookmarks = new List<BookmarkDto>
            {
                new BookmarkDto { Value = "Chapter 1", PageNumber = 1 }
            }
        };
        var history = new PdfHistory { PdfGuid = guid };

        _mockHistoryRepository.Setup(r => r.GetByGuidAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);
        _mockS3Service.Setup(s => s.DownloadPdfAsync(guid, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.AddBookmarks(guid, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region AssignVariables Tests

    [Test]
    public async Task AssignVariables_WhenPdfNotFound_ShouldReturn404()
    {
        // Arrange
        var guid = "non-existent-guid";
        var request = new AssignVariablesRequest
        {
            Variables = new Dictionary<string, string>
            {
                { "Name", "John Doe" }
            }
        };

        _mockHistoryRepository.Setup(r => r.GetByGuidAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PdfHistory?)null);

        // Act
        var result = await _controller.AssignVariables(guid, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task AssignVariables_WhenDownloadFails_ShouldReturn404()
    {
        // Arrange
        var guid = "test-guid";
        var request = new AssignVariablesRequest
        {
            Variables = new Dictionary<string, string> { { "Name", "John" } }
        };
        var history = new PdfHistory { PdfGuid = guid };

        _mockHistoryRepository.Setup(r => r.GetByGuidAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);
        _mockS3Service.Setup(s => s.DownloadPdfAsync(guid, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.AssignVariables(guid, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region AddKeywords Tests

    [Test]
    public async Task AddKeywords_WhenPdfNotFound_ShouldReturn404()
    {
        // Arrange
        var guid = "non-existent-guid";
        var request = new AddKeywordsRequest
        {
            Keywords = new List<string> { "Important", "Contract" }
        };

        _mockHistoryRepository.Setup(r => r.GetByGuidAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PdfHistory?)null);

        // Act
        var result = await _controller.AddKeywords(guid, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task AddKeywords_WhenDownloadFails_ShouldReturn404()
    {
        // Arrange
        var guid = "test-guid";
        var request = new AddKeywordsRequest
        {
            Keywords = new List<string> { "Important" }
        };
        var history = new PdfHistory { PdfGuid = guid };

        _mockHistoryRepository.Setup(r => r.GetByGuidAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);
        _mockS3Service.Setup(s => s.DownloadPdfAsync(guid, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.AddKeywords(guid, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task AddKeywords_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var guid = "test-guid";
        var request = new AddKeywordsRequest
        {
            Keywords = new List<string> { "Important" }
        };
        var history = new PdfHistory { PdfGuid = guid };

        _mockHistoryRepository.Setup(r => r.GetByGuidAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);
        _mockS3Service.Setup(s => s.DownloadPdfAsync(guid, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockPdfService.Setup(p => p.AddKeywordsAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.AddKeywords(guid, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region MergePdfs Tests

    [Test]
    public async Task MergePdfs_WhenSourcePdfNotFound_ShouldReturn404()
    {
        // Arrange
        var request = new MergePdfsRequest
        {
            ApplicationName = "TestApp",
            PdfGuids = new List<string> { "guid-1", "guid-2" }
        };

        _mockS3Service.Setup(s => s.DownloadPdfAsync("guid-1", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.MergePdfs(request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task MergePdfs_WhenMergeFails_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new MergePdfsRequest
        {
            ApplicationName = "TestApp",
            PdfGuids = new List<string> { "guid-1", "guid-2" }
        };

        _mockS3Service.Setup(s => s.DownloadPdfAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockPdfService.Setup(p => p.MergePdfsAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.MergePdfs(request);

        // Assert
        // Fixed: Now properly checks file existence before delete, returns BadRequest
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}
