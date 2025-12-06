using StackOverGrave.Api.Models;
using StackOverGrave.Api.Exceptions;
using System.Text;
using System.Text.Json;

namespace StackOverGrave.Api.Services;

public class MigrationGuideService : IMigrationGuideService
{
    private readonly ILogger<MigrationGuideService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _openAiApiKey;
    private const int MaxGuideTokens = 3000;
    private const int ApproximateCharsPerToken = 4;

    public MigrationGuideService(
        ILogger<MigrationGuideService> logger,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _openAiApiKey = configuration["OpenAI:ApiKey"] ?? "";
        
        if (string.IsNullOrWhiteSpace(_openAiApiKey))
        {
            _logger.LogWarning("⚠️ OpenAI API key is not configured for migration guide generation");
        }
    }

    public async Task<string> GenerateMigrationGuideAsync(
        RepositoryConversionResult conversionResult,
        RepositoryAnalysisResult analysisResult,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📝 Generating migration guide for {TotalFiles} total files, {ConvertedFiles} converted",
            analysisResult.TotalFiles, conversionResult.ConvertedFiles.Count);

        try
        {
            // Build the prompt for AI guide generation
            var prompt = BuildGuidePrompt(conversionResult, analysisResult);
            
            // Call OpenAI to generate the guide content
            var aiGeneratedContent = await CallOpenAiForGuideAsync(prompt, cancellationToken);
            
            // Format the guide as structured markdown
            var formattedGuide = FormatMigrationGuide(
                aiGeneratedContent,
                conversionResult,
                analysisResult
            );

            _logger.LogInformation("✅ Migration guide generated successfully");
            return formattedGuide;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to generate migration guide");
            
            // Return a basic fallback guide
            return GenerateFallbackGuide(conversionResult, analysisResult);
        }
    }

    /// <summary>
    /// Builds the OpenAI prompt for migration guide generation
    /// </summary>
    private string BuildGuidePrompt(
        RepositoryConversionResult conversionResult,
        RepositoryAnalysisResult analysisResult)
    {
        var promptBuilder = new StringBuilder();

        promptBuilder.AppendLine("You are an expert software migration consultant. Generate a comprehensive migration guide for a legacy code repository conversion.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("PROJECT SUMMARY:");
        promptBuilder.AppendLine($"- Original Technology: {GetTechnologyName(analysisResult.DetectedTechnology)}");
        promptBuilder.AppendLine($"- Target Technology: {GetTargetTechnologyName(analysisResult.DetectedTechnology)}");
        promptBuilder.AppendLine($"- Total Files: {analysisResult.TotalFiles}");
        promptBuilder.AppendLine($"- Total Lines of Code: {analysisResult.TotalLinesOfCode:N0}");
        promptBuilder.AppendLine($"- Files Converted: {conversionResult.ConvertedFiles.Count}");
        promptBuilder.AppendLine($"- Files Remaining: {analysisResult.TotalFiles - conversionResult.ConvertedFiles.Count}");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine("CONVERTED FILES:");
        foreach (var file in conversionResult.ConvertedFiles.Take(15))
        {
            promptBuilder.AppendLine($"- {file.OriginalPath} → {file.ConvertedPath}");
            
            if (file.MigrationNotes.Any())
            {
                promptBuilder.AppendLine($"  Notes: {string.Join("; ", file.MigrationNotes.Take(2))}");
            }
        }
        promptBuilder.AppendLine();

        // Include key dependencies
        var allDependencies = conversionResult.ConvertedFiles
            .SelectMany(f => f.Dependencies)
            .Distinct()
            .ToList();

        if (allDependencies.Any())
        {
            promptBuilder.AppendLine("DEPENDENCIES IDENTIFIED:");
            foreach (var dep in allDependencies.Take(10))
            {
                promptBuilder.AppendLine($"- {dep}");
            }
            promptBuilder.AppendLine();
        }

        promptBuilder.AppendLine("TASK:");
        promptBuilder.AppendLine("Generate a migration guide with the following sections:");
        promptBuilder.AppendLine("1. Find-and-Replace Patterns: Common code patterns that need updating");
        promptBuilder.AppendLine("2. File-by-File Notes: Key changes and considerations for each converted file");
        promptBuilder.AppendLine("3. Manual Steps: Steps needed to complete the migration for unconverted files");
        promptBuilder.AppendLine("4. Breaking Changes: Important changes that may affect functionality");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Respond in JSON format:");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"findAndReplacePatterns\": [{\"from\": \"old pattern\", \"to\": \"new pattern\", \"description\": \"why\"}],");
        promptBuilder.AppendLine("  \"fileNotes\": [{\"file\": \"path\", \"notes\": [\"note1\", \"note2\"]}],");
        promptBuilder.AppendLine("  \"manualSteps\": [\"step1\", \"step2\"],");
        promptBuilder.AppendLine("  \"breakingChanges\": [\"change1\", \"change2\"],");
        promptBuilder.AppendLine("  \"warnings\": [\"warning1\", \"warning2\"]");
        promptBuilder.AppendLine("}");

        return promptBuilder.ToString();
    }

    /// <summary>
    /// Calls OpenAI API to generate migration guide content
    /// </summary>
    private async Task<MigrationGuideContent> CallOpenAiForGuideAsync(
        string prompt,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_openAiApiKey))
        {
            throw new AiServiceException(
                "AI service is not configured",
                new { missingConfiguration = "OpenAI API key" },
                "Please contact the administrator to configure the OpenAI API key"
            );
        }

        var request = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = "You are a software migration expert. Always respond with valid JSON." },
                new { role = "user", content = prompt }
            },
            temperature = 0.3,
            max_tokens = MaxGuideTokens
        };

        var requestJson = JsonSerializer.Serialize(request);
        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

        _logger.LogInformation("📡 Calling OpenAI API for migration guide generation...");
        
        var response = await _httpClient.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            httpContent,
            cancellationToken
        );

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("❌ OpenAI API error: {Status} - {Response}", response.StatusCode, responseJson);
            throw new AiServiceException(
                "AI service request failed",
                new { statusCode = response.StatusCode.ToString() },
                "Please try again in a few moments"
            );
        }
        
        var responseObj = JsonSerializer.Deserialize<OpenAiResponse>(responseJson, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        var messageContent = responseObj?.Choices?[0]?.Message?.Content;
        if (string.IsNullOrEmpty(messageContent))
        {
            throw new AiServiceException(
                "AI service returned an empty response",
                new { error = "Empty content" },
                "Please try again in a few moments"
            );
        }

        return ParseGuideContent(messageContent);
    }

    /// <summary>
    /// Parses the AI response into structured guide content
    /// </summary>
    private MigrationGuideContent ParseGuideContent(string response)
    {
        try
        {
            // Extract JSON from markdown code blocks if present
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var content = JsonSerializer.Deserialize<MigrationGuideContent>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return content ?? new MigrationGuideContent();
            }

            return new MigrationGuideContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse migration guide content");
            return new MigrationGuideContent();
        }
    }

    /// <summary>
    /// Formats the migration guide as structured markdown
    /// </summary>
    private string FormatMigrationGuide(
        MigrationGuideContent content,
        RepositoryConversionResult conversionResult,
        RepositoryAnalysisResult analysisResult)
    {
        var guide = new StringBuilder();

        // Header
        guide.AppendLine("# Migration Guide");
        guide.AppendLine();
        guide.AppendLine($"*Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*");
        guide.AppendLine();

        // Project Summary
        guide.AppendLine("## Project Summary");
        guide.AppendLine();
        guide.AppendLine($"- **Original Technology**: {GetTechnologyName(analysisResult.DetectedTechnology)}");
        guide.AppendLine($"- **Target Technology**: {GetTargetTechnologyName(analysisResult.DetectedTechnology)}");
        guide.AppendLine($"- **Total Files**: {analysisResult.TotalFiles}");
        guide.AppendLine($"- **Total Lines of Code**: {analysisResult.TotalLinesOfCode:N0}");
        guide.AppendLine($"- **Files Converted**: {conversionResult.ConvertedFiles.Count}");
        guide.AppendLine($"- **Files Remaining**: {analysisResult.TotalFiles - conversionResult.ConvertedFiles.Count}");
        guide.AppendLine($"- **Estimated Cost**: ${conversionResult.EstimatedCost:F2}");
        
        var estimatedHours = CalculateEstimatedCompletionTime(analysisResult.TotalFiles - conversionResult.ConvertedFiles.Count);
        guide.AppendLine($"- **Estimated Completion Time**: {estimatedHours} hours");
        guide.AppendLine();

        // Converted Files List
        guide.AppendLine("## Converted Files");
        guide.AppendLine();
        guide.AppendLine("The following files have been automatically converted:");
        guide.AppendLine();
        
        foreach (var file in conversionResult.ConvertedFiles)
        {
            guide.AppendLine($"- `{file.OriginalPath}` → `{file.ConvertedPath}`");
        }
        guide.AppendLine();

        // Find and Replace Patterns
        if (content.FindAndReplacePatterns.Any())
        {
            guide.AppendLine("## Find and Replace Patterns");
            guide.AppendLine();
            guide.AppendLine("Apply these patterns to remaining files:");
            guide.AppendLine();
            
            foreach (var pattern in content.FindAndReplacePatterns)
            {
                guide.AppendLine($"### {pattern.Description}");
                guide.AppendLine();
                guide.AppendLine("```");
                guide.AppendLine($"Find:    {pattern.From}");
                guide.AppendLine($"Replace: {pattern.To}");
                guide.AppendLine("```");
                guide.AppendLine();
            }
        }

        // File-by-File Notes
        if (content.FileNotes.Any())
        {
            guide.AppendLine("## File-by-File Notes");
            guide.AppendLine();
            
            foreach (var fileNote in content.FileNotes)
            {
                guide.AppendLine($"### {fileNote.File}");
                guide.AppendLine();
                
                foreach (var note in fileNote.Notes)
                {
                    guide.AppendLine($"- {note}");
                }
                guide.AppendLine();
            }
        }

        // Manual Steps Required
        guide.AppendLine("## Manual Steps Required");
        guide.AppendLine();
        
        if (content.ManualSteps.Any())
        {
            foreach (var step in content.ManualSteps)
            {
                guide.AppendLine($"1. {step}");
            }
        }
        else
        {
            guide.AppendLine("1. Review and convert remaining unconverted files");
            guide.AppendLine("2. Update configuration files and connection strings");
            guide.AppendLine("3. Test all converted functionality");
            guide.AppendLine("4. Update deployment scripts and CI/CD pipelines");
        }
        guide.AppendLine();

        // Dependencies to Install
        var allDependencies = conversionResult.ConvertedFiles
            .SelectMany(f => f.Dependencies)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        if (allDependencies.Any())
        {
            guide.AppendLine("## Dependencies to Install");
            guide.AppendLine();
            
            foreach (var dep in allDependencies)
            {
                guide.AppendLine($"- {dep}");
            }
            guide.AppendLine();
        }

        // Breaking Changes
        if (content.BreakingChanges.Any())
        {
            guide.AppendLine("## Breaking Changes");
            guide.AppendLine();
            guide.AppendLine("⚠️ **Important**: The following breaking changes require attention:");
            guide.AppendLine();
            
            foreach (var change in content.BreakingChanges)
            {
                guide.AppendLine($"- {change}");
            }
            guide.AppendLine();
        }

        // Warnings
        if (content.Warnings.Any())
        {
            guide.AppendLine("## Warnings");
            guide.AppendLine();
            
            foreach (var warning in content.Warnings)
            {
                guide.AppendLine($"⚠️ {warning}");
            }
            guide.AppendLine();
        }

        // Additional Resources
        guide.AppendLine("## Additional Resources");
        guide.AppendLine();
        guide.AppendLine(GetAdditionalResources(analysisResult.DetectedTechnology));
        guide.AppendLine();

        // Footer
        guide.AppendLine("---");
        guide.AppendLine();
        guide.AppendLine("*This migration guide was automatically generated by StackOverGrave. Please review all converted code carefully before deploying to production.*");

        return guide.ToString();
    }

    /// <summary>
    /// Generates a fallback guide when AI generation fails
    /// </summary>
    private string GenerateFallbackGuide(
        RepositoryConversionResult conversionResult,
        RepositoryAnalysisResult analysisResult)
    {
        var guide = new StringBuilder();

        guide.AppendLine("# Migration Guide");
        guide.AppendLine();
        guide.AppendLine($"*Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*");
        guide.AppendLine();

        guide.AppendLine("## Project Summary");
        guide.AppendLine();
        guide.AppendLine($"- **Original Technology**: {GetTechnologyName(analysisResult.DetectedTechnology)}");
        guide.AppendLine($"- **Target Technology**: {GetTargetTechnologyName(analysisResult.DetectedTechnology)}");
        guide.AppendLine($"- **Total Files**: {analysisResult.TotalFiles}");
        guide.AppendLine($"- **Files Converted**: {conversionResult.ConvertedFiles.Count}");
        guide.AppendLine($"- **Files Remaining**: {analysisResult.TotalFiles - conversionResult.ConvertedFiles.Count}");
        guide.AppendLine();

        guide.AppendLine("## Converted Files");
        guide.AppendLine();
        foreach (var file in conversionResult.ConvertedFiles)
        {
            guide.AppendLine($"- `{file.OriginalPath}` → `{file.ConvertedPath}`");
        }
        guide.AppendLine();

        guide.AppendLine("## Next Steps");
        guide.AppendLine();
        guide.AppendLine("1. Review all converted files for accuracy");
        guide.AppendLine("2. Convert remaining files manually or using the same conversion approach");
        guide.AppendLine("3. Update configuration and connection strings");
        guide.AppendLine("4. Test all functionality thoroughly");
        guide.AppendLine("5. Update deployment scripts");
        guide.AppendLine();

        guide.AppendLine("---");
        guide.AppendLine("*Note: Detailed migration guidance could not be generated. Please review the converted code carefully.*");

        return guide.ToString();
    }

    /// <summary>
    /// Gets the display name for a technology type
    /// </summary>
    private string GetTechnologyName(TechnologyType tech)
    {
        return tech switch
        {
            TechnologyType.VB6 => "Visual Basic 6",
            TechnologyType.ActionScript => "ActionScript/Flash",
            TechnologyType.Silverlight => "Silverlight",
            TechnologyType.DotNetFramework => ".NET Framework",
            _ => tech.ToString()
        };
    }

    /// <summary>
    /// Gets the target technology name based on source technology
    /// </summary>
    private string GetTargetTechnologyName(TechnologyType sourceTech)
    {
        return sourceTech switch
        {
            TechnologyType.VB6 => ".NET 8 C#",
            TechnologyType.ActionScript => "TypeScript/Angular 17",
            TechnologyType.Silverlight => "Blazor/Angular 17",
            TechnologyType.DotNetFramework => ".NET 8",
            _ => ".NET 8"
        };
    }

    /// <summary>
    /// Calculates estimated completion time for remaining files
    /// </summary>
    private string CalculateEstimatedCompletionTime(int remainingFiles)
    {
        // Estimate 30-60 minutes per file for manual conversion
        var minHours = remainingFiles * 0.5;
        var maxHours = remainingFiles * 1.0;
        
        if (minHours < 1)
            return "< 1";
        
        return $"{minHours:F0}-{maxHours:F0}";
    }

    /// <summary>
    /// Gets additional resources based on technology type
    /// </summary>
    private string GetAdditionalResources(TechnologyType tech)
    {
        return tech switch
        {
            TechnologyType.VB6 => @"- [VB6 to C# Migration Guide](https://docs.microsoft.com/en-us/dotnet/visual-basic/programming-guide/)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [C# Language Reference](https://docs.microsoft.com/en-us/dotnet/csharp/)",

            TechnologyType.ActionScript => @"- [ActionScript to TypeScript Migration](https://www.typescriptlang.org/docs/)
- [Angular Documentation](https://angular.io/docs)
- [HTML5 Canvas API](https://developer.mozilla.org/en-US/docs/Web/API/Canvas_API)",

            TechnologyType.Silverlight => @"- [Silverlight Migration Guide](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/migration/)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Angular Material](https://material.angular.io/)",

            TechnologyType.DotNetFramework => @"- [.NET Framework to .NET 8 Migration](https://docs.microsoft.com/en-us/dotnet/core/porting/)
- [Breaking Changes in .NET](https://docs.microsoft.com/en-us/dotnet/core/compatibility/)
- [ASP.NET Core Migration](https://docs.microsoft.com/en-us/aspnet/core/migration/)",

            _ => "- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)"
        };
    }

    // Helper classes for OpenAI response parsing
    private class OpenAiResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    private class Message
    {
        [System.Text.Json.Serialization.JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}

/// <summary>
/// Structured content for migration guide
/// </summary>
public class MigrationGuideContent
{
    public List<FindReplacePattern> FindAndReplacePatterns { get; set; } = new();
    public List<FileNote> FileNotes { get; set; } = new();
    public List<string> ManualSteps { get; set; } = new();
    public List<string> BreakingChanges { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class FindReplacePattern
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class FileNote
{
    public string File { get; set; } = string.Empty;
    public List<string> Notes { get; set; } = new();
}
