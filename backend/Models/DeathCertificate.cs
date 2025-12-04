namespace StackOverGrave.Api.Models;

public class DeathCertificate
{
    public TechnologyType Technology { get; set; }
    public string OriginalFilename { get; set; } = string.Empty;
    public string? DetectedVersion { get; set; }
    public DateTime DeprecatedDate { get; set; }
    public string CauseOfDeath { get; set; } = string.Empty;
    public FileStatistics FileStats { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class FileStatistics
{
    public int LinesOfCode { get; set; }
    public long FileSize { get; set; }
    public string Complexity { get; set; } = "Low";
}
