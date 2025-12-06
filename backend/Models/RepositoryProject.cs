namespace StackOverGrave.Api.Models;

public class RepositoryProject
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public RepositorySource Source { get; set; }
    public string? SourceUrl { get; set; }
    public string OriginalFilename { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public ProjectSize ProjectSize { get; set; }
    public TechnologyType SourceTechnology { get; set; }
    public TechnologyType TargetTechnology { get; set; }
    public int TotalFiles { get; set; }
    public int TotalLinesOfCode { get; set; }
    public int ConvertedFiles { get; set; }
    public decimal EstimatedCost { get; set; }
    public string? PackagePath { get; set; }
    
    // Navigation property
    public ICollection<RepositoryFile> Files { get; set; } = new List<RepositoryFile>();
}
