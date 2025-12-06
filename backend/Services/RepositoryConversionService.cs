using StackOverGrave.Api.Models;
using System.Text;

namespace StackOverGrave.Api.Services;

public class RepositoryConversionService : IRepositoryConversionService
{
    private readonly IAiConversionService _aiConversionService;
    private readonly ILogger<RepositoryConversionService> _logger;
    private const int MaxContextTokens = 2000;
    private const int ApproximateCharsPerToken = 4;
    private const decimal CostPerToken = 0.0000015m; // gpt-3.5-turbo pricing

    public RepositoryConversionService(
        IAiConversionService aiConversionService,
        ILogger<RepositoryConversionService> logger)
    {
        _aiConversionService = aiConversionService;
        _logger = logger;
    }

    public async Task<RepositoryConversionResult> ConvertRepositoryAsync(
        RepositoryAnalysisResult analysis,
        string extractPath,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🚀 Starting repository conversion for {FileCount} files", 
            analysis.PrioritizedFiles.Count(f => f.SelectedForConversion));

        var result = new RepositoryConversionResult();
        var convertedFiles = new List<ConvertedFile>();
        var conversionContext = new List<ConvertedFile>();

        // Get files selected for conversion and order them
        var filesToConvert = OrderFilesForConversion(
            analysis.PrioritizedFiles.Where(f => f.SelectedForConversion).ToList()
        );

        int currentIndex = 0;
        int totalFiles = filesToConvert.Count;

        foreach (var fileInfo in filesToConvert)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("⚠️ Conversion cancelled by user");
                break;
            }

            currentIndex++;
            var percentage = (double)currentIndex / totalFiles * 100;

            progress?.Report(new ConversionProgress
            {
                CurrentFileIndex = currentIndex,
                TotalFiles = totalFiles,
                CurrentFileName = fileInfo.RelativePath,
                Percentage = percentage,
                Message = $"Converting {fileInfo.RelativePath}..."
            });

            try
            {
                var filePath = Path.Combine(extractPath, fileInfo.RelativePath);
                
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("⚠️ File not found: {Path}", filePath);
                    result.Errors.Add(new FileConversionError
                    {
                        FilePath = fileInfo.RelativePath,
                        ErrorMessage = "File not found"
                    });
                    continue;
                }

                var originalCode = await File.ReadAllTextAsync(filePath, cancellationToken);
                
                // Build contextual prompt with previous conversions
                var convertedFile = await ConvertFileWithContextAsync(
                    fileInfo,
                    originalCode,
                    analysis.DetectedTechnology,
                    conversionContext,
                    cancellationToken
                );

                convertedFiles.Add(convertedFile);
                conversionContext.Add(convertedFile);

                // Track token usage (approximate) - Requirement 15.4
                var tokensUsed = EstimateTokens(originalCode) + EstimateTokens(convertedFile.ConvertedCode);
                result.TotalTokensUsed += tokensUsed;

                // Log token usage for each conversion - Requirement 15.4
                _logger.LogInformation("✅ Converted {File} ({Index}/{Total}) - Tokens: {Tokens}, Cost: ${Cost:F4}", 
                    fileInfo.RelativePath, currentIndex, totalFiles, tokensUsed, tokensUsed * CostPerToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to convert {File}", fileInfo.RelativePath);
                result.Errors.Add(new FileConversionError
                {
                    FilePath = fileInfo.RelativePath,
                    ErrorMessage = ex.Message
                });
                
                // Continue with next file despite error
                continue;
            }
        }

        result.ConvertedFiles = convertedFiles;
        // Calculate estimated cost based on token usage - Requirement 15.4
        result.EstimatedCost = result.TotalTokensUsed * CostPerToken;

        // Log OpenAI token usage for cost tracking - Requirement 15.4
        _logger.LogInformation("✅ Conversion complete: {Success}/{Total} files, {Tokens} tokens, ${Cost:F2} estimated cost", 
            convertedFiles.Count, totalFiles, result.TotalTokensUsed, result.EstimatedCost);

        // Validate cost stays within target range - Requirements 15.1, 15.2, 15.5
        const decimal MaxCostTarget = 7.0m;
        if (result.EstimatedCost > MaxCostTarget)
        {
            _logger.LogWarning("⚠️ Cost exceeds target: ${Cost:F2} > ${Target:F2} for {Files} files", 
                result.EstimatedCost, MaxCostTarget, totalFiles);
        }
        else
        {
            _logger.LogInformation("✅ Cost within target: ${Cost:F2} <= ${Target:F2}", 
                result.EstimatedCost, MaxCostTarget);
        }

        return result;
    }

    /// <summary>
    /// Orders files for conversion: models → services → UI components
    /// </summary>
    private List<RepositoryFileInfo> OrderFilesForConversion(List<RepositoryFileInfo> files)
    {
        return files.OrderByDescending(f =>
        {
            var path = f.RelativePath.ToLowerInvariant();
            
            // Models first (highest priority)
            if (path.Contains("/models/") || path.Contains("\\models\\") ||
                path.Contains("/entities/") || path.Contains("\\entities\\"))
                return 3;
            
            // Services second
            if (path.Contains("/services/") || path.Contains("\\services\\") ||
                path.Contains("/business/") || path.Contains("\\business\\"))
                return 2;
            
            // UI components last
            if (path.Contains("/controllers/") || path.Contains("\\controllers\\") ||
                path.Contains("/forms/") || path.Contains("\\forms\\") ||
                path.Contains("/views/") || path.Contains("\\views\\"))
                return 1;
            
            // Everything else
            return 0;
        })
        .ThenByDescending(f => f.CriticalityScore)
        .ToList();
    }

    /// <summary>
    /// Converts a file with context from previously converted files
    /// </summary>
    private async Task<ConvertedFile> ConvertFileWithContextAsync(
        RepositoryFileInfo fileInfo,
        string originalCode,
        TechnologyType sourceTech,
        List<ConvertedFile> previousConversions,
        CancellationToken cancellationToken)
    {
        // Build context from previous conversions
        var contextualPrompt = BuildContextualPrompt(
            fileInfo,
            originalCode,
            sourceTech,
            previousConversions
        );

        // Determine target technology
        var targetTech = DetermineTargetTechnology(sourceTech);

        // Call AI service with contextual prompt
        var conversionResult = await _aiConversionService.ConvertCodeAsync(
            contextualPrompt,
            sourceTech,
            targetTech,
            cancellationToken
        );

        // Determine converted file path
        var convertedPath = DetermineConvertedPath(fileInfo.RelativePath, sourceTech, targetTech);

        return new ConvertedFile
        {
            OriginalPath = fileInfo.RelativePath,
            ConvertedPath = convertedPath,
            OriginalCode = originalCode,
            ConvertedCode = conversionResult.ConvertedCode,
            Dependencies = conversionResult.Dependencies,
            MigrationNotes = conversionResult.MigrationNotes
        };
    }

    /// <summary>
    /// Builds a contextual prompt that includes relevant previous conversions
    /// </summary>
    private string BuildContextualPrompt(
        RepositoryFileInfo fileInfo,
        string originalCode,
        TechnologyType sourceTech,
        List<ConvertedFile> previousConversions)
    {
        var promptBuilder = new StringBuilder();

        // Add context from previous conversions if available
        if (previousConversions.Any())
        {
            promptBuilder.AppendLine("CONTEXT FROM PREVIOUSLY CONVERTED FILES:");
            promptBuilder.AppendLine();

            // Prioritize recent and related files
            var relevantContext = GetRelevantContext(fileInfo, previousConversions);
            
            foreach (var context in relevantContext)
            {
                promptBuilder.AppendLine($"// From {context.ConvertedPath}:");
                
                // Include key parts of converted code (interfaces, class signatures, key methods)
                var contextSnippet = ExtractKeyElements(context.ConvertedCode);
                promptBuilder.AppendLine(contextSnippet);
                promptBuilder.AppendLine();
            }

            promptBuilder.AppendLine("---");
            promptBuilder.AppendLine();
        }

        // Add the current file to convert
        promptBuilder.AppendLine($"FILE TO CONVERT: {fileInfo.RelativePath}");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine(originalCode);

        return promptBuilder.ToString();
    }

    /// <summary>
    /// Gets relevant context from previous conversions, prioritizing related files
    /// </summary>
    private List<ConvertedFile> GetRelevantContext(
        RepositoryFileInfo currentFile,
        List<ConvertedFile> previousConversions)
    {
        var maxContextSize = MaxContextTokens * ApproximateCharsPerToken / 2; // Reserve half for context
        var relevantFiles = new List<ConvertedFile>();
        var currentSize = 0;

        // Get directory of current file
        var currentDir = Path.GetDirectoryName(currentFile.RelativePath) ?? "";

        // Prioritize files from same directory, then models, then recent files
        var prioritized = previousConversions
            .OrderByDescending(f =>
            {
                var fileDir = Path.GetDirectoryName(f.OriginalPath) ?? "";
                if (fileDir == currentDir) return 3; // Same directory
                if (f.ConvertedPath.Contains("/Models/") || f.ConvertedPath.Contains("\\Models\\")) return 2; // Models
                return 1; // Recent files
            })
            .ThenByDescending(f => previousConversions.IndexOf(f)) // Most recent first
            .ToList();

        foreach (var file in prioritized)
        {
            var snippet = ExtractKeyElements(file.ConvertedCode);
            var snippetSize = snippet.Length;

            if (currentSize + snippetSize > maxContextSize)
                break;

            relevantFiles.Add(file);
            currentSize += snippetSize;
        }

        return relevantFiles;
    }

    /// <summary>
    /// Extracts key elements from code (class signatures, interfaces, key methods)
    /// </summary>
    private string ExtractKeyElements(string code)
    {
        var lines = code.Split('\n');
        var keyLines = new List<string>();
        var inClass = false;
        var braceCount = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Include using statements, namespaces, class/interface declarations
            if (trimmed.StartsWith("using ") ||
                trimmed.StartsWith("namespace ") ||
                trimmed.Contains("class ") ||
                trimmed.Contains("interface ") ||
                trimmed.Contains("enum ") ||
                trimmed.Contains("public ") && (trimmed.Contains("(") || trimmed.Contains("{")))
            {
                keyLines.Add(line);
                
                if (trimmed.Contains("{"))
                {
                    inClass = true;
                    braceCount = 1;
                }
            }
            else if (inClass)
            {
                // Track braces to know when class ends
                braceCount += trimmed.Count(c => c == '{');
                braceCount -= trimmed.Count(c => c == '}');

                if (braceCount == 0)
                {
                    keyLines.Add(line);
                    inClass = false;
                }
            }

            // Limit to reasonable size
            if (keyLines.Count > 50)
                break;
        }

        return string.Join("\n", keyLines);
    }

    /// <summary>
    /// Determines target technology based on source technology
    /// </summary>
    private TechnologyType DetermineTargetTechnology(TechnologyType sourceTech)
    {
        return sourceTech switch
        {
            TechnologyType.VB6 => TechnologyType.DotNetFramework, // Will be .NET 8 C#
            TechnologyType.ActionScript => TechnologyType.DotNetFramework, // TypeScript/Angular (reusing enum)
            TechnologyType.Silverlight => TechnologyType.DotNetFramework, // Blazor/Angular (reusing enum)
            TechnologyType.DotNetFramework => TechnologyType.DotNetFramework, // .NET 8
            _ => TechnologyType.DotNetFramework
        };
    }

    /// <summary>
    /// Determines the converted file path based on source and target technologies
    /// </summary>
    private string DetermineConvertedPath(string originalPath, TechnologyType sourceTech, TechnologyType targetTech)
    {
        var fileName = Path.GetFileNameWithoutExtension(originalPath);
        var directory = Path.GetDirectoryName(originalPath) ?? "";

        return sourceTech switch
        {
            TechnologyType.VB6 => Path.Combine(directory, $"{fileName}.cs"),
            TechnologyType.ActionScript => Path.Combine(directory, $"{fileName}.component.ts"),
            TechnologyType.Silverlight => Path.Combine(directory, $"{fileName}.component.ts"),
            TechnologyType.DotNetFramework => Path.Combine(directory, $"{fileName}.cs"),
            _ => Path.Combine(directory, $"{fileName}.cs")
        };
    }

    /// <summary>
    /// Estimates token count from text (approximate)
    /// </summary>
    private int EstimateTokens(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        // Rough estimate: 1 token ≈ 4 characters
        return text.Length / ApproximateCharsPerToken;
    }
}
