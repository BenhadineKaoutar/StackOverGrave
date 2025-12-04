namespace StackOverGrave.Api.Models;

public class MigrationData
{
    public List<string> MigrationNotes { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    public List<string> BreakingChanges { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
