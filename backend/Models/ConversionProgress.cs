namespace StackOverGrave.Api.Models;

public class ConversionProgress
{
    public int CurrentFileIndex { get; set; }
    public int TotalFiles { get; set; }
    public string CurrentFileName { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public string Message { get; set; } = string.Empty;
}
