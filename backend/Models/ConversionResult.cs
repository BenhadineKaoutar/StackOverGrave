namespace StackOverGrave.Api.Models;

public class ConversionResult
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string OriginalCode { get; set; } = string.Empty;
    public string ConvertedCode { get; set; } = string.Empty;
    public string MigrationNotes { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public Project? Project { get; set; }
}
