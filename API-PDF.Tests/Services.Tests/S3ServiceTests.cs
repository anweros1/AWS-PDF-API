using API_PDF.Models;
using API_PDF.Services;
using API_PDF.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace API_PDF.Tests.Services.Tests;

[TestFixture]
public class S3ServiceTests
{
    private Mock<ILogger<S3Service>> _loggerMock;
    private Mock<IOptions<AwsSettings>> _awsSettingsMock;
    private Mock<IOptions<PdfSettings>> _pdfSettingsMock;
    private AwsSettings _awsSettings;
    private PdfSettings _pdfSettings;
    private string _testTempFolder;
    private string _testFallbackFolder;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<S3Service>>();
        
        // Create test folders
        _testTempFolder = Path.Combine(Path.GetTempPath(), "PdfApiTests", "Temp");
        _testFallbackFolder = Path.Combine(Path.GetTempPath(), "PdfApiTests", "Fallback");
        
        Directory.CreateDirectory(_testTempFolder);
        Directory.CreateDirectory(_testFallbackFolder);

        _awsSettings = new AwsSettings
        {
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            BucketName = "test-bucket",
            Region = "us-east-1"
        };

        _pdfSettings = new PdfSettings
        {
            TempFolder = _testTempFolder,
            LocalFallbackFolder = _testFallbackFolder
        };

        _awsSettingsMock = new Mock<IOptions<AwsSettings>>();
        _awsSettingsMock.Setup(x => x.Value).Returns(_awsSettings);

        _pdfSettingsMock = new Mock<IOptions<PdfSettings>>();
        _pdfSettingsMock.Setup(x => x.Value).Returns(_pdfSettings);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test folders
        if (Directory.Exists(_testTempFolder))
        {
            Directory.Delete(_testTempFolder, true);
        }
        if (Directory.Exists(_testFallbackFolder))
        {
            Directory.Delete(_testFallbackFolder, true);
        }
    }

    private string CreateTestPdfFile(string fileName = "test.pdf")
    {
        var filePath = Path.Combine(_testTempFolder, fileName);
        File.WriteAllText(filePath, "%PDF-1.4\nTest PDF Content");
        return filePath;
    }

    [Test]
    public async Task UploadPdfAsync_WhenS3Unavailable_ShouldFallbackToLocalStorage()
    {
        // Arrange
        var service = CreateServiceWithInvalidS3Credentials();
        var testFile = CreateTestPdfFile();
        var pdfGuid = Guid.NewGuid().ToString();

        // Act
        var (url, isStoredInS3) = await service.UploadPdfAsync(testFile, pdfGuid);

        // Assert
        isStoredInS3.Should().BeFalse("S3 is unavailable, should fallback to local");
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain(_testFallbackFolder);
        
        // Verify file was copied to fallback folder
        var expectedPath = Path.Combine(_testFallbackFolder, $"{pdfGuid}.pdf");
        File.Exists(expectedPath).Should().BeTrue();
    }

    [Test]
    public async Task DownloadPdfAsync_WhenFileExistsLocally_ShouldDownloadSuccessfully()
    {
        // Arrange
        var service = CreateServiceWithInvalidS3Credentials();
        var pdfGuid = Guid.NewGuid().ToString();
        
        // Create a file in fallback folder
        var sourceFile = Path.Combine(_testFallbackFolder, $"{pdfGuid}.pdf");
        File.WriteAllText(sourceFile, "%PDF-1.4\nTest Content");

        var destinationPath = Path.Combine(_testTempFolder, "downloaded.pdf");

        // Act
        var result = await service.DownloadPdfAsync(pdfGuid, destinationPath);

        // Assert
        result.Should().BeTrue();
        File.Exists(destinationPath).Should().BeTrue();
        File.ReadAllText(destinationPath).Should().Contain("Test Content");
    }

    [Test]
    public async Task DownloadPdfAsync_WhenFileDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateServiceWithInvalidS3Credentials();
        var pdfGuid = Guid.NewGuid().ToString();
        var destinationPath = Path.Combine(_testTempFolder, "downloaded.pdf");

        // Act
        var result = await service.DownloadPdfAsync(pdfGuid, destinationPath);

        // Assert
        result.Should().BeFalse();
        File.Exists(destinationPath).Should().BeFalse();
    }

    [Test]
    public async Task DeletePdfAsync_WhenFileExistsLocally_ShouldDeleteSuccessfully()
    {
        // Arrange
        var service = CreateServiceWithInvalidS3Credentials();
        var pdfGuid = Guid.NewGuid().ToString();
        
        // Create a file in fallback folder
        var filePath = Path.Combine(_testFallbackFolder, $"{pdfGuid}.pdf");
        File.WriteAllText(filePath, "Test Content");

        // Act
        var result = await service.DeletePdfAsync(pdfGuid);

        // Assert
        result.Should().BeTrue();
        File.Exists(filePath).Should().BeFalse();
    }

    [Test]
    public async Task DeletePdfAsync_WhenFileDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateServiceWithInvalidS3Credentials();
        var pdfGuid = Guid.NewGuid().ToString();

        // Act
        var result = await service.DeletePdfAsync(pdfGuid);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task PdfExistsAsync_WhenFileExistsLocally_ShouldReturnTrue()
    {
        // Arrange
        var service = CreateServiceWithInvalidS3Credentials();
        var pdfGuid = Guid.NewGuid().ToString();
        
        // Create a file in fallback folder
        var filePath = Path.Combine(_testFallbackFolder, $"{pdfGuid}.pdf");
        File.WriteAllText(filePath, "Test Content");

        // Act
        var result = await service.PdfExistsAsync(pdfGuid);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task PdfExistsAsync_WhenFileDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateServiceWithInvalidS3Credentials();
        var pdfGuid = Guid.NewGuid().ToString();

        // Act
        var result = await service.PdfExistsAsync(pdfGuid);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task GetPdfUrlAsync_WhenFileExistsLocally_ShouldReturnLocalPath()
    {
        // Arrange
        var service = CreateServiceWithInvalidS3Credentials();
        var pdfGuid = Guid.NewGuid().ToString();
        
        // Create a file in fallback folder
        var filePath = Path.Combine(_testFallbackFolder, $"{pdfGuid}.pdf");
        File.WriteAllText(filePath, "Test Content");

        // Act
        var (url, isStoredInS3) = await service.GetPdfUrlAsync(pdfGuid);

        // Assert
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain(pdfGuid);
        isStoredInS3.Should().BeFalse();
    }

    [Test]
    public async Task GetPdfUrlAsync_WhenFileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var service = CreateServiceWithInvalidS3Credentials();
        var pdfGuid = Guid.NewGuid().ToString();

        // Act
        var (url, isStoredInS3) = await service.GetPdfUrlAsync(pdfGuid);

        // Assert
        url.Should().BeNull();
        isStoredInS3.Should().BeFalse();
    }

    [Test]
    public async Task IsS3AvailableAsync_WhenCredentialsInvalid_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateServiceWithInvalidS3Credentials();

        // Act
        var result = await service.IsS3AvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void Constructor_ShouldCreateFallbackFolderIfNotExists()
    {
        // Arrange
        var newFallbackFolder = Path.Combine(Path.GetTempPath(), "PdfApiTests", "NewFallback");
        _pdfSettings.LocalFallbackFolder = newFallbackFolder;

        // Ensure folder doesn't exist
        if (Directory.Exists(newFallbackFolder))
        {
            Directory.Delete(newFallbackFolder, true);
        }

        // Act
        var service = new S3Service(_loggerMock.Object, _awsSettingsMock.Object, _pdfSettingsMock.Object);

        // Assert
        Directory.Exists(newFallbackFolder).Should().BeTrue();

        // Cleanup
        Directory.Delete(newFallbackFolder, true);
    }

    private S3Service CreateServiceWithInvalidS3Credentials()
    {
        // Use invalid credentials to force local fallback
        _awsSettings.AccessKey = "invalid";
        _awsSettings.SecretKey = "invalid";
        return new S3Service(_loggerMock.Object, _awsSettingsMock.Object, _pdfSettingsMock.Object);
    }
}
