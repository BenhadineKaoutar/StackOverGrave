namespace StackOverGrave.Api.Models;

public class RepositoryFile
{
    public Guid Id { get; set; }
    public Guid RepositoryProjectId { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public int CriticalityScore { get; set; }
    public bool SelectedForConversion { get; set; }
    public bool ConversionSucceeded { get; set; }
    public string? ConversionError { get; set; }
    public string? ConvertedPath { get; set; }
    
    // Navigation property
    public RepositoryProject? RepositoryProject { get; set; }
}
