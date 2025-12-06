namespace StackOverGrave.Api.Models;

public class RepositoryAnalysisResult
{
    public int TotalFiles { get; set; }
    public int TotalLinesOfCode { get; set; }
    public TechnologyType DetectedTechnology { get; set; }
    public ProjectSize ProjectSize { get; set; }
    public List<RepositoryFileInfo> PrioritizedFiles { get; set; } = new();
    public List<string> ValidationErrors { get; set; } = new();
}
