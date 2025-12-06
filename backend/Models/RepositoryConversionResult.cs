namespace StackOverGrave.Api.Models;

public class RepositoryConversionResult
{
    public List<ConvertedFile> ConvertedFiles { get; set; } = new();
    public List<FileConversionError> Errors { get; set; } = new();
    public int TotalTokensUsed { get; set; }
    public decimal EstimatedCost { get; set; }
}
