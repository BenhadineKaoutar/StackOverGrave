namespace StackOverGrave.Api.Models;

public class Project
{
    public Guid Id { get; set; }
    public string OriginalFilename { get; set; } = string.Empty;
    public TechnologyType Technology { get; set; }
    public DateTime UploadedAt { get; set; }
    public ProjectStatus Status { get; set; }
    public long FileSize { get; set; }
    public int LinesOfCode { get; set; }
    public string FilePath { get; set; } = string.Empty;
}
