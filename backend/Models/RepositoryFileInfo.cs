namespace StackOverGrave.Api.Models;

public class RepositoryFileInfo
{
    public string RelativePath { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public int CriticalityScore { get; set; }
    public bool SelectedForConversion { get; set; }
}
