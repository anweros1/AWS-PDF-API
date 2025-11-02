using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using API_PDF.Models;
using API_PDF.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace API_PDF.Services;

/// <summary>
/// Service for managing PDF files in AWS S3 with automatic local fallback
/// </summary>
public class S3Service : IS3Service
{
    private readonly ILogger<S3Service> _logger;
    private readonly AwsSettings _awsSettings;
    private readonly PdfSettings _pdfSettings;
    private readonly IAmazonS3? _s3Client;
    private readonly bool _useLocalFallback;

    public S3Service(
        ILogger<S3Service> logger,
        IOptions<AwsSettings> awsSettings,
        IOptions<PdfSettings> pdfSettings)
    {
        _logger = logger;
        _awsSettings = awsSettings.Value;
        _pdfSettings = pdfSettings.Value;

        // Ensure fallback folder exists
        if (!Directory.Exists(_pdfSettings.LocalFallbackFolder))
        {
            Directory.CreateDirectory(_pdfSettings.LocalFallbackFolder);
            _logger.LogInformation("Created local fallback folder: {Folder}", _pdfSettings.LocalFallbackFolder);
        }

        // Try to initialize S3 client
        try
        {
            if (!string.IsNullOrWhiteSpace(_awsSettings.AccessKey) &&
                !string.IsNullOrWhiteSpace(_awsSettings.SecretKey) &&
                !string.IsNullOrWhiteSpace(_awsSettings.BucketName))
            {
                var region = RegionEndpoint.GetBySystemName(_awsSettings.Region);
                _s3Client = new AmazonS3Client(_awsSettings.AccessKey, _awsSettings.SecretKey, region);
                _useLocalFallback = false;
                _logger.LogInformation("S3 client initialized for bucket: {Bucket}", _awsSettings.BucketName);
            }
            else
            {
                _useLocalFallback = true;
                _logger.LogWarning("AWS credentials not configured. Using local fallback storage.");
            }
        }
        catch (Exception ex)
        {
            _useLocalFallback = true;
            _logger.LogError(ex, "Failed to initialize S3 client. Using local fallback storage.");
        }
    }

    public async Task<(string Url, bool IsStoredInS3)> UploadPdfAsync(
        string filePath,
        string pdfGuid,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        // Try S3 upload first
        if (!_useLocalFallback && _s3Client != null)
        {
            try
            {
                var s3Key = $"pdfs/{pdfGuid}.pdf";
                
                using var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(new TransferUtilityUploadRequest
                {
                    BucketName = _awsSettings.BucketName,
                    Key = s3Key,
                    FilePath = filePath,
                    CannedACL = S3CannedACL.Private
                }, cancellationToken);

                // Generate pre-signed URL for the uploaded file (valid for 1 hour)
                var presignedUrl = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = _awsSettings.BucketName,
                    Key = s3Key,
                    Expires = DateTime.UtcNow.AddHours(1),
                    Verb = HttpVerb.GET
                });
                
                _logger.LogInformation("Uploaded PDF {PdfGuid} to S3 with pre-signed URL", pdfGuid);
                return (presignedUrl, true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to upload to S3, falling back to local storage for {PdfGuid}", pdfGuid);
            }
        }

        // Fallback to local storage
        var localPath = Path.Combine(_pdfSettings.LocalFallbackFolder, $"{pdfGuid}.pdf");
        File.Copy(filePath, localPath, overwrite: true);
        
        _logger.LogInformation("Saved PDF {PdfGuid} to local fallback: {LocalPath}", pdfGuid, localPath);
        return (localPath, false);
    }

    public async Task<bool> DownloadPdfAsync(
        string pdfGuid,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        // Try S3 download first
        if (!_useLocalFallback && _s3Client != null)
        {
            try
            {
                var s3Key = $"pdfs/{pdfGuid}.pdf";
                
                using var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.DownloadAsync(new TransferUtilityDownloadRequest
                {
                    BucketName = _awsSettings.BucketName,
                    Key = s3Key,
                    FilePath = destinationPath
                }, cancellationToken);

                _logger.LogInformation("Downloaded PDF {PdfGuid} from S3 to {Destination}", pdfGuid, destinationPath);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("PDF {PdfGuid} not found in S3, checking local fallback", pdfGuid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download from S3 for {PdfGuid}, checking local fallback", pdfGuid);
            }
        }

        // Try local fallback
        var localPath = Path.Combine(_pdfSettings.LocalFallbackFolder, $"{pdfGuid}.pdf");
        if (File.Exists(localPath))
        {
            File.Copy(localPath, destinationPath, overwrite: true);
            _logger.LogInformation("Downloaded PDF {PdfGuid} from local fallback to {Destination}", pdfGuid, destinationPath);
            return true;
        }

        _logger.LogWarning("PDF {PdfGuid} not found in S3 or local fallback", pdfGuid);
        return false;
    }

    public async Task<bool> DeletePdfAsync(string pdfGuid, CancellationToken cancellationToken = default)
    {
        var deleted = false;

        // Try S3 delete first
        if (!_useLocalFallback && _s3Client != null)
        {
            try
            {
                var s3Key = $"pdfs/{pdfGuid}.pdf";
                
                await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = _awsSettings.BucketName,
                    Key = s3Key
                }, cancellationToken);

                _logger.LogInformation("Deleted PDF {PdfGuid} from S3", pdfGuid);
                deleted = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete from S3 for {PdfGuid}", pdfGuid);
            }
        }

        // Try local fallback delete
        var localPath = Path.Combine(_pdfSettings.LocalFallbackFolder, $"{pdfGuid}.pdf");
        if (File.Exists(localPath))
        {
            File.Delete(localPath);
            _logger.LogInformation("Deleted PDF {PdfGuid} from local fallback", pdfGuid);
            deleted = true;
        }

        return deleted;
    }

    public async Task<bool> PdfExistsAsync(string pdfGuid, CancellationToken cancellationToken = default)
    {
        // Check S3 first
        if (!_useLocalFallback && _s3Client != null)
        {
            try
            {
                var s3Key = $"pdfs/{pdfGuid}.pdf";
                
                await _s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = _awsSettings.BucketName,
                    Key = s3Key
                }, cancellationToken);

                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Not in S3, check local
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking S3 for {PdfGuid}", pdfGuid);
            }
        }

        // Check local fallback
        var localPath = Path.Combine(_pdfSettings.LocalFallbackFolder, $"{pdfGuid}.pdf");
        return File.Exists(localPath);
    }

    public async Task<(string? Url, bool IsStoredInS3)> GetPdfUrlAsync(
        string pdfGuid,
        CancellationToken cancellationToken = default)
    {
        // Check S3 first
        if (!_useLocalFallback && _s3Client != null)
        {
            try
            {
                var s3Key = $"pdfs/{pdfGuid}.pdf";
                
                // Verify file exists
                await _s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = _awsSettings.BucketName,
                    Key = s3Key
                }, cancellationToken);

                // Generate pre-signed URL for private objects (valid for 1 hour)
                var presignedUrl = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = _awsSettings.BucketName,
                    Key = s3Key,
                    Expires = DateTime.UtcNow.AddHours(1),
                    Verb = HttpVerb.GET
                });

                _logger.LogDebug("Generated pre-signed URL for {PdfGuid}, expires in 1 hour", pdfGuid);
                return (presignedUrl, true);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("PDF {PdfGuid} not found in S3", pdfGuid);
                // Not in S3, check local
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting S3 URL for {PdfGuid}", pdfGuid);
            }
        }

        // Check local fallback
        var localPath = Path.Combine(_pdfSettings.LocalFallbackFolder, $"{pdfGuid}.pdf");
        if (File.Exists(localPath))
        {
            return (localPath, false);
        }

        return (null, false);
    }

    public async Task<bool> IsS3AvailableAsync()
    {
        if (_useLocalFallback || _s3Client == null)
        {
            return false;
        }

        try
        {
            // Try to list buckets as a connectivity test
            await _s3Client.ListBucketsAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "S3 availability check failed");
            return false;
        }
    }
}
