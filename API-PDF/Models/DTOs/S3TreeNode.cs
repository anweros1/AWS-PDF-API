namespace API_PDF.Models.DTOs;

/// <summary>
/// Represents a node in the S3 bucket tree structure
/// </summary>
public class S3TreeNode
{
    /// <summary>
    /// Name of the file or folder
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full path/key in S3
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Type of node: "folder" or "file"
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes (only for files)
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// Last modified date (only for files)
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Child nodes (only for folders)
    /// </summary>
    public List<S3TreeNode> Children { get; set; } = new();

    /// <summary>
    /// Number of items in this folder (only for folders)
    /// </summary>
    public int ItemCount { get; set; }
}

/// <summary>
/// Response for S3 bucket tree listing
/// </summary>
public class S3TreeResponse
{
    /// <summary>
    /// Root nodes of the tree
    /// </summary>
    public List<S3TreeNode> Tree { get; set; } = new();

    /// <summary>
    /// Total number of files in the bucket
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Total number of folders in the bucket
    /// </summary>
    public int TotalFolders { get; set; }

    /// <summary>
    /// Total size of all files in bytes
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Bucket name
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// Prefix/path filter applied (if any)
    /// </summary>
    public string? Prefix { get; set; }
}
