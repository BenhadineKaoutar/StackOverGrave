namespace StackOverGrave.Api.Models;

public class ConvertedFile
{
    public string OriginalPath { get; set; } = string.Empty;
    public string ConvertedPath { get; set; } = string.Empty;
    public string OriginalCode { get; set; } = string.Empty;
    public string ConvertedCode { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
    public List<string> MigrationNotes { get; set; } = new();
}
