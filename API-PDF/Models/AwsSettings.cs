using System.ComponentModel.DataAnnotations;

namespace API_PDF.Models;

/// <summary>
/// AWS configuration settings
/// </summary>
public class AwsSettings
{
    public const string SectionName = "AWS";

    /// <summary>
    /// AWS Access Key ID
    /// </summary>
    [Required(ErrorMessage = "AWS Access Key is required")]
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// AWS Secret Access Key
    /// </summary>
    [Required(ErrorMessage = "AWS Secret Key is required")]
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// S3 Bucket Name
    /// </summary>
    [Required(ErrorMessage = "S3 Bucket Name is required")]
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// AWS Region (e.g., us-east-1)
    /// </summary>
    [Required(ErrorMessage = "AWS Region is required")]
    public string Region { get; set; } = "us-east-1";
}
