namespace StackOverGrave.Api.Models;

/// <summary>
/// Request model for Git repository import
/// </summary>
public class GitImportRequest
{
    public string Url { get; set; } = string.Empty;
}
