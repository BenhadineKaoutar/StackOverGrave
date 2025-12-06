using StackOverGrave.Api.Models;
using StackOverGrave.Api.Exceptions;

namespace StackOverGrave.Api.Services;

public class RepositoryAnalysisService : IRepositoryAnalysisService
{
    private readonly ITechnologyDetectionService _technologyDetectionService;
    private readonly ILogger<RepositoryAnalysisService> _logger;
    
    // File extensions to include
    private static readonly HashSet<string> ValidExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".vb", ".bas", ".cls", ".frm", // VB6
        ".as",                          // ActionScript
        ".xaml",                        // Silverlight
        ".cs"                           // C#
    };
    
    // Directories to exclude
    private static readonly HashSet<string> ExcludedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "obj", "node_modules", ".git", ".vs", "packages"
    };
    
    // Filename patterns to exclude
    private static readonly string[] ExcludedPatterns = new[]
    {
        ".Designer.", ".generated.", ".g.cs", ".g.i.cs"
    };
    
    // Limits
    private const int MaxFiles = 50;
    private const int MaxTotalLinesOfCode = 15000;
    private const int MaxSingleFileLinesOfCode = 1000;
    private const int SmallProjectMaxFiles = 20;
    private const int SmallProjectMaxLOC = 5000;
    private const int MediumProjectMaxFiles = 15; // Files to convert for medium projects
    
    public RepositoryAnalysisService(
        ITechnologyDetectionService technologyDetectionService,
        ILogger<RepositoryAnalysisService> logger)
    {
        _technologyDetectionService = technologyDetectionService;
        _logger = logger;
    }
    
    public async Task<RepositoryAnalysisResult> AnalyzeRepositoryAsync(
        string extractPath, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting repository analysis for path: {Path}", extractPath);
        
        var result = new RepositoryAnalysisResult();
        
        // Step 1: Scan and filter files
        var scannedFiles = ScanDirectory(extractPath, extractPath);
        _logger.LogInformation("Scanned {Count} files after filtering", scannedFiles.Count);
        
        // Step 2: Count lines and validate
        var fileInfoList = new List<RepositoryFileInfo>();
        
        foreach (var filePath in scannedFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);
            var lineCount = CountNonEmptyLines(content);
            var relativePath = Path.GetRelativePath(extractPath, filePath);
            
            fileInfoList.Add(new RepositoryFileInfo
            {
                RelativePath = relativePath,
                LineCount = lineCount,
                CriticalityScore = 0 // Will be calculated later
            });
        }
        
        result.TotalFiles = fileInfoList.Count;
        result.TotalLinesOfCode = fileInfoList.Sum(f => f.LineCount);
        
        // Step 3: Validate limits
        ValidateLimits(result, fileInfoList);
        
        if (result.ValidationErrors.Any())
        {
            _logger.LogWarning("Repository validation failed with {Count} errors", result.ValidationErrors.Count);
            result.ProjectSize = ProjectSize.TooLarge;
            return result;
        }
        
        // Step 4: Calculate criticality scores
        foreach (var fileInfo in fileInfoList)
        {
            var fullPath = Path.Combine(extractPath, fileInfo.RelativePath);
            var content = await File.ReadAllTextAsync(fullPath, cancellationToken);
            fileInfo.CriticalityScore = CalculateCriticalityScore(fileInfo.RelativePath, content, fileInfo.LineCount);
        }
        
        // Step 5: Sort by criticality score (descending)
        result.PrioritizedFiles = fileInfoList.OrderByDescending(f => f.CriticalityScore).ToList();
        
        // Step 6: Determine project size and select files for conversion
        DetermineProjectSizeAndSelectFiles(result);
        
        // Step 7: Detect technology
        result.DetectedTechnology = await DetectTechnologyAsync(extractPath, scannedFiles, cancellationToken);
        
        _logger.LogInformation(
            "Analysis complete: {Files} files, {LOC} LOC, {Tech} technology, {Size} project",
            result.TotalFiles,
            result.TotalLinesOfCode,
            result.DetectedTechnology,
            result.ProjectSize);
        
        return result;
    }
    
    private List<string> ScanDirectory(string rootPath, string currentPath)
    {
        var files = new List<string>();
        
        try
        {
            // Get all files in current directory
            var directoryFiles = Directory.GetFiles(currentPath)
                .Where(f => IsValidFile(f))
                .ToList();
            
            files.AddRange(directoryFiles);
            
            // Recursively scan subdirectories
            var subdirectories = Directory.GetDirectories(currentPath)
                .Where(d => !IsExcludedDirectory(d))
                .ToList();
            
            foreach (var subdirectory in subdirectories)
            {
                files.AddRange(ScanDirectory(rootPath, subdirectory));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied to directory: {Path}", currentPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning directory: {Path}", currentPath);
        }
        
        return files;
    }
    
    private bool IsValidFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        var fileName = Path.GetFileName(filePath);
        
        // Check extension
        if (!ValidExtensions.Contains(extension))
            return false;
        
        // Check excluded patterns
        if (ExcludedPatterns.Any(pattern => fileName.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            return false;
        
        return true;
    }
    
    private bool IsExcludedDirectory(string directoryPath)
    {
        var directoryName = Path.GetFileName(directoryPath);
        return ExcludedDirectories.Contains(directoryName);
    }
    
    private int CountNonEmptyLines(string content)
    {
        return content.Split('\n')
            .Count(line => !string.IsNullOrWhiteSpace(line));
    }
    
    private void ValidateLimits(RepositoryAnalysisResult result, List<RepositoryFileInfo> files)
    {
        // Validate file count (requirement 13.2)
        if (result.TotalFiles > MaxFiles)
        {
            var errorMessage = $"Repository contains too many files ({result.TotalFiles} files)";
            result.ValidationErrors.Add(errorMessage);
            
            throw new RepositorySizeLimitException(
                errorMessage,
                new { 
                    actualFiles = result.TotalFiles,
                    maxFiles = MaxFiles,
                    difference = result.TotalFiles - MaxFiles
                },
                "Consider splitting the repository, removing build artifacts, or selecting specific directories to convert"
            );
        }
        
        // Validate total LOC (requirement 13.3)
        if (result.TotalLinesOfCode > MaxTotalLinesOfCode)
        {
            var errorMessage = $"Repository contains too many lines of code ({result.TotalLinesOfCode:N0} LOC)";
            result.ValidationErrors.Add(errorMessage);
            
            throw new RepositorySizeLimitException(
                errorMessage,
                new { 
                    actualLOC = result.TotalLinesOfCode,
                    maxLOC = MaxTotalLinesOfCode,
                    difference = result.TotalLinesOfCode - MaxTotalLinesOfCode
                },
                "Focus on specific modules or refactor to reduce the codebase size before conversion"
            );
        }
        
        // Validate individual file LOC (requirement 13.3)
        var oversizedFiles = files.Where(f => f.LineCount > MaxSingleFileLinesOfCode).ToList();
        if (oversizedFiles.Any())
        {
            var fileList = oversizedFiles.Select(f => new { file = f.RelativePath, lines = f.LineCount }).ToList();
            var fileNames = string.Join(", ", oversizedFiles.Select(f => f.RelativePath));
            var errorMessage = $"Repository contains files that are too large";
            result.ValidationErrors.Add(errorMessage);
            
            throw new RepositorySizeLimitException(
                errorMessage,
                new { 
                    oversizedFiles = fileList,
                    maxLinesPerFile = MaxSingleFileLinesOfCode,
                    count = oversizedFiles.Count
                },
                $"Please refactor these files into smaller modules: {fileNames}"
            );
        }
    }
    
    private int CalculateCriticalityScore(string relativePath, string content, int lineCount)
    {
        int score = 0;
        
        // Entry point detection (+100)
        if (content.Contains("Sub Main", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("static void Main", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("public static void Main", StringComparison.OrdinalIgnoreCase))
        {
            score += 100;
        }
        
        // Directory-based scoring
        var normalizedPath = relativePath.Replace('\\', '/');
        
        if (normalizedPath.Contains("/Models/", StringComparison.OrdinalIgnoreCase) ||
            normalizedPath.Contains("/Entities/", StringComparison.OrdinalIgnoreCase))
        {
            score += 80;
        }
        else if (normalizedPath.Contains("/Services/", StringComparison.OrdinalIgnoreCase) ||
                 normalizedPath.Contains("/Business/", StringComparison.OrdinalIgnoreCase))
        {
            score += 60;
        }
        else if (normalizedPath.Contains("/Controllers/", StringComparison.OrdinalIgnoreCase) ||
                 normalizedPath.Contains("/Forms/", StringComparison.OrdinalIgnoreCase))
        {
            score += 40;
        }
        
        // Size-based scoring
        if (lineCount < 100)
        {
            score += 20;
        }
        else if (lineCount > 1000)
        {
            score -= 1000;
        }
        
        return score;
    }
    
    private void DetermineProjectSizeAndSelectFiles(RepositoryAnalysisResult result)
    {
        // Determine project size
        if (result.TotalFiles <= SmallProjectMaxFiles && result.TotalLinesOfCode <= SmallProjectMaxLOC)
        {
            result.ProjectSize = ProjectSize.Small;
            // Select all files for small projects
            foreach (var file in result.PrioritizedFiles)
            {
                file.SelectedForConversion = true;
            }
            _logger.LogInformation("Classified as Small project - all {Count} files selected", result.TotalFiles);
        }
        else
        {
            result.ProjectSize = ProjectSize.Medium;
            // Select top 15 files for medium projects
            var filesToConvert = result.PrioritizedFiles.Take(MediumProjectMaxFiles).ToList();
            foreach (var file in filesToConvert)
            {
                file.SelectedForConversion = true;
            }
            _logger.LogInformation(
                "Classified as Medium project - {Selected} of {Total} files selected for conversion",
                filesToConvert.Count,
                result.TotalFiles);
        }
    }
    
    private async Task<TechnologyType> DetectTechnologyAsync(
        string extractPath,
        List<string> scannedFiles,
        CancellationToken cancellationToken)
    {
        // Count files by technology
        var technologyCounts = new Dictionary<TechnologyType, int>();
        
        foreach (var filePath in scannedFiles.Take(10)) // Sample first 10 files for performance
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var fileName = Path.GetFileName(filePath);
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);
            
            var tech = _technologyDetectionService.DetectTechnology(fileName, content);
            
            if (tech != TechnologyType.Unknown)
            {
                technologyCounts.TryGetValue(tech, out var count);
                technologyCounts[tech] = count + 1;
            }
        }
        
        // Return the most common technology
        if (technologyCounts.Any())
        {
            return technologyCounts.OrderByDescending(kvp => kvp.Value).First().Key;
        }
        
        return TechnologyType.Unknown;
    }
}
