using System.Text.RegularExpressions;
using StackOverGrave.Api.Models;

namespace StackOverGrave.Api.Services;

public class TechnologyDetectionService : ITechnologyDetectionService
{
    public async Task<DeathCertificate> AnalyzeFileAsync(string filePath)
    {
        var filename = Path.GetFileName(filePath);
        var content = await File.ReadAllTextAsync(filePath);
        var fileInfo = new FileInfo(filePath);
        
        var technology = DetectTechnology(filename, content);
        var stats = CalculateStatistics(content);
        
        return new DeathCertificate
        {
            Technology = technology,
            OriginalFilename = filename,
            DeprecatedDate = GetDeprecatedDate(technology),
            CauseOfDeath = GetCauseOfDeath(technology),
            FileStats = new FileStatistics
            {
                LinesOfCode = stats.loc,
                FileSize = fileInfo.Length,
                Complexity = stats.complexity
            }
        };
    }

    public TechnologyType DetectTechnology(string filename, string content)
    {
        var ext = Path.GetExtension(filename).ToLower();
        
        // VB6 Detection
        if (ext == ".vb" || ext == ".frm" || ext == ".bas" || ext == ".cls")
        {
            if (Regex.IsMatch(content, @"Attribute VB_Name|Option Explicit|MsgBox|InputBox"))
                return TechnologyType.VB6;
        }
        
        // ActionScript Detection
        if (ext == ".as")
        {
            if (Regex.IsMatch(content, @"package\s*\{|import flash\.|extends Sprite|extends MovieClip"))
                return TechnologyType.ActionScript;
        }
        
        // Silverlight Detection
        if (ext == ".xaml")
        {
            if (content.Contains("schemas.microsoft.com/winfx/2006/xaml/presentation"))
                return TechnologyType.Silverlight;
        }
        
        // Old .NET Framework Detection
        if (ext == ".csproj")
        {
            if (Regex.IsMatch(content, @"<TargetFramework>net4[0-7]</TargetFramework>|<TargetFrameworkVersion>v4\.[0-7]"))
                return TechnologyType.DotNetFramework;
        }
        
        return TechnologyType.Unknown;
    }

    private (int loc, string complexity) CalculateStatistics(string content)
    {
        var lines = content.Split('\n');
        var loc = lines.Count(l => !string.IsNullOrWhiteSpace(l) && !l.TrimStart().StartsWith("//"));
        
        var complexity = loc switch
        {
            < 100 => "Low",
            < 500 => "Medium",
            _ => "High"
        };
        
        return (loc, complexity);
    }

    private DateTime GetDeprecatedDate(TechnologyType tech) => tech switch
    {
        TechnologyType.VB6 => new DateTime(2008, 4, 8),
        TechnologyType.ActionScript => new DateTime(2020, 12, 31),
        TechnologyType.Silverlight => new DateTime(2021, 10, 12),
        _ => DateTime.Now
    };

    private string GetCauseOfDeath(TechnologyType tech) => tech switch
    {
        TechnologyType.VB6 => "Abandoned by Microsoft in favor of .NET",
        TechnologyType.ActionScript => "Killed by Steve Jobs and HTML5",
        TechnologyType.Silverlight => "Murdered by responsive web design",
        TechnologyType.DotNetFramework => "Replaced by .NET Core/.NET 5+",
        _ => "Unknown cause"
    };
}
