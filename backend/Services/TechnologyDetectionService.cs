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
        var fullName = filename.ToLower();
        
        // ASP.NET Web Forms Detection (check before VB6)
        if (fullName.EndsWith(".aspx.vb") || fullName.EndsWith(".aspx.cs") || 
            fullName.EndsWith(".ascx.vb") || fullName.EndsWith(".ascx.cs") ||
            fullName.EndsWith(".ashx.vb") || fullName.EndsWith(".ashx.cs"))
        {
            return TechnologyType.DotNetFramework;
        }
        
        // ASP.NET Web Forms markup
        if (ext == ".aspx" || ext == ".ascx" || ext == ".master")
        {
            if (content.Contains("<%@") || content.Contains("runat=\"server\""))
                return TechnologyType.DotNetFramework;
        }
        
        // VB.NET / C# code files with .NET Framework indicators
        if (ext == ".vb" || ext == ".cs")
        {
            // Check for .NET Framework patterns
            if (Regex.IsMatch(content, @"Imports System\.Web|using System\.Web|Inherits System\.Web\.UI\.Page|: System\.Web\.UI\.Page"))
                return TechnologyType.DotNetFramework;
            
            // Check for VB6 patterns (only if not .NET)
            if (ext == ".vb" && Regex.IsMatch(content, @"Attribute VB_Name|Option Explicit.*\n.*Sub Main\(\)|MsgBox\s*\(|InputBox\s*\("))
                return TechnologyType.VB6;
        }
        
        // VB6 specific file extensions
        if (ext == ".frm" || ext == ".bas" || ext == ".cls")
        {
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
        
        // Old .NET Framework Detection (project files)
        if (ext == ".csproj" || ext == ".vbproj")
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
