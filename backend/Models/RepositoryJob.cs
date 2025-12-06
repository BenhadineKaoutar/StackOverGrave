namespace StackOverGrave.Api.Models;

/// <summary>
/// Represents a background repository conversion job with status tracking
/// </summary>
public class RepositoryJob
{
    public Guid Id { get; set; }
    public JobStatus Status { get; set; }
    public int ProgressPercentage { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? TempDirectory { get; set; }
    public string? TempZipPath { get; set; }
    public string? ResultPackagePath { get; set; }
    public RepositoryAnalysisResult? AnalysisResult { get; set; }
    public RepositoryConversionResult? ConversionResult { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Job status enumeration with progress stages
/// </summary>
public enum JobStatus
{
    Downloading,    // 0-20%
    Extracting,     // 20-40%
    Analyzing,      // 40-60%
    Converting,     // 60-80%
    Packaging,      // 80-90%
    Completed,      // 100%
    Failed
}
