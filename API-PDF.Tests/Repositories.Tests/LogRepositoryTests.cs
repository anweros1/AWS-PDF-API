using API_PDF.Data;
using API_PDF.Models.Entities;
using API_PDF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace API_PDF.Tests.Repositories.Tests;

[TestFixture]
public class LogRepositoryTests
{
    private ApplicationDbContext _context;
    private LogRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new LogRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddLogAsync_ShouldAddLogToDatabase()
    {
        // Arrange
        var log = new ApiCallLog
        {
            PdfGuid = "test-guid-123",
            ApplicationName = "TestApp",
            Endpoint = "/api/test",
            HttpMethod = "POST",
            DurationMs = 100,
            IsSuccess = true,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddLogAsync(log);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        
        var savedLog = await _context.ApiCallLogs.FindAsync(result.Id);
        savedLog.Should().NotBeNull();
        savedLog!.PdfGuid.Should().Be("test-guid-123");
    }

    [Test]
    public async Task GetLogsByPdfGuidAsync_ShouldReturnLogsForSpecificGuid()
    {
        // Arrange
        var guid1 = "guid-1";
        var guid2 = "guid-2";

        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = guid1, ApplicationName = "App1", Endpoint = "/test", HttpMethod = "GET", IsSuccess = true });
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = guid1, ApplicationName = "App1", Endpoint = "/test", HttpMethod = "POST", IsSuccess = true });
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = guid2, ApplicationName = "App1", Endpoint = "/test", HttpMethod = "GET", IsSuccess = true });

        // Act
        var logs = await _repository.GetLogsByPdfGuidAsync(guid1);

        // Assert
        logs.Should().HaveCount(2);
        logs.Should().OnlyContain(l => l.PdfGuid == guid1);
    }

    [Test]
    public async Task GetLogsByApplicationAsync_ShouldReturnLogsForSpecificApplication()
    {
        // Arrange
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = "guid1", ApplicationName = "App1", Endpoint = "/test", HttpMethod = "GET", IsSuccess = true });
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = "guid2", ApplicationName = "App1", Endpoint = "/test", HttpMethod = "POST", IsSuccess = true });
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = "guid3", ApplicationName = "App2", Endpoint = "/test", HttpMethod = "GET", IsSuccess = true });

        // Act
        var logs = await _repository.GetLogsByApplicationAsync("App1");

        // Assert
        logs.Should().HaveCount(2);
        logs.Should().OnlyContain(l => l.ApplicationName == "App1");
    }

    [Test]
    public async Task GetLogsByDateRangeAsync_ShouldReturnLogsWithinRange()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var yesterday = now.AddDays(-1);
        var tomorrow = now.AddDays(1);

        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = "guid1", ApplicationName = "App1", Endpoint = "/test", HttpMethod = "GET", IsSuccess = true, Timestamp = yesterday });
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = "guid2", ApplicationName = "App1", Endpoint = "/test", HttpMethod = "GET", IsSuccess = true, Timestamp = now });
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = "guid3", ApplicationName = "App1", Endpoint = "/test", HttpMethod = "GET", IsSuccess = true, Timestamp = tomorrow });

        // Act
        var logs = await _repository.GetLogsByDateRangeAsync(yesterday.AddHours(-1), now.AddHours(1));

        // Assert
        logs.Should().HaveCount(2);
    }

    [Test]
    public async Task GetFailedLogsAsync_ShouldReturnOnlyFailedLogs()
    {
        // Arrange
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = "guid1", ApplicationName = "App1", Endpoint = "/test", HttpMethod = "GET", IsSuccess = true });
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = "guid2", ApplicationName = "App1", Endpoint = "/test", HttpMethod = "GET", IsSuccess = false, ErrorMessage = "Error 1" });
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = "guid3", ApplicationName = "App1", Endpoint = "/test", HttpMethod = "GET", IsSuccess = false, ErrorMessage = "Error 2" });

        // Act
        var logs = await _repository.GetFailedLogsAsync();

        // Assert
        logs.Should().HaveCount(2);
        logs.Should().OnlyContain(l => !l.IsSuccess);
    }

    [Test]
    public async Task GetLogStatisticsAsync_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var guid = "test-guid";
        
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = guid, ApplicationName = "App1", Endpoint = "/test", HttpMethod = "GET", IsSuccess = true, DurationMs = 100 });
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = guid, ApplicationName = "App1", Endpoint = "/test", HttpMethod = "GET", IsSuccess = true, DurationMs = 200 });
        await _repository.AddLogAsync(new ApiCallLog { PdfGuid = guid, ApplicationName = "App1", Endpoint = "/test", HttpMethod = "GET", IsSuccess = false, DurationMs = 150 });

        // Act
        var (totalCalls, successfulCalls, failedCalls, averageDuration) = await _repository.GetLogStatisticsAsync(guid);

        // Assert
        totalCalls.Should().Be(3);
        successfulCalls.Should().Be(2);
        failedCalls.Should().Be(1);
        averageDuration.Should().Be(150); // (100 + 200 + 150) / 3
    }

    [Test]
    public async Task GetLogStatisticsAsync_WithNoLogs_ShouldReturnZeros()
    {
        // Act
        var (totalCalls, successfulCalls, failedCalls, averageDuration) = await _repository.GetLogStatisticsAsync("non-existent-guid");

        // Assert
        totalCalls.Should().Be(0);
        successfulCalls.Should().Be(0);
        failedCalls.Should().Be(0);
        averageDuration.Should().Be(0);
    }
}
