using StackOverGrave.Api.Models;
using StackOverGrave.Api.Exceptions;

namespace StackOverGrave.Api.Services;

/// <summary>
/// Manages background repository conversion jobs with progress tracking
/// </summary>
public class RepositoryJobProcessor : IRepositoryJobProcessor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RepositoryJobProcessor> _logger;

    public RepositoryJobProcessor(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<RepositoryJobProcessor> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Starts a new repository conversion job from Git URL
    /// </summary>
    public async Task<Guid> StartGitImportJobAsync(string gitUrl)
    {
        var jobId = Guid.NewGuid();
        
        var job = new RepositoryJob
        {
            Id = jobId,
            Status = JobStatus.Downloading,
            ProgressPercentage = 0,
            StatusMessage = "Starting Git repository download...",
            StartedAt = DateTime.UtcNow
        };

        JobStore.AddJob(job);
        _logger.LogInformation("🚀 Started Git import job {JobId} for URL: {Url}", jobId, gitUrl);

        // Start background processing with Task.Run (requirement 12.8)
        // Fire and forget - errors are handled within the task
        _ = Task.Run(async () =>
        {
            try
            {
                await ProcessGitImportJobAsync(jobId, gitUrl);
            }
            catch (Exception ex)
            {
                // Comprehensive error handling (requirement 12.8)
                _logger.LogError(ex, "❌ Unhandled exception in background task for job {JobId}", jobId);
                FailJob(jobId, new[] { "An unexpected error occurred during processing" });
            }
        });

        return jobId;
    }

    /// <summary>
    /// Starts a new repository conversion job from uploaded ZIP
    /// </summary>
    public async Task<Guid> StartZipUploadJobAsync(string zipFilePath, Guid? jobId = null)
    {
        var actualJobId = jobId ?? Guid.NewGuid();
        
        var job = new RepositoryJob
        {
            Id = actualJobId,
            Status = JobStatus.Extracting,
            ProgressPercentage = 20,
            StatusMessage = "Starting ZIP extraction...",
            StartedAt = DateTime.UtcNow,
            TempZipPath = zipFilePath
        };

        JobStore.AddJob(job);
        _logger.LogInformation("🚀 Started ZIP upload job {JobId} for file: {File}", actualJobId, zipFilePath);

        // Start background processing with Task.Run (requirement 12.8)
        // Fire and forget - errors are handled within the task
        _ = Task.Run(async () =>
        {
            try
            {
                await ProcessZipUploadJobAsync(actualJobId, zipFilePath);
            }
            catch (Exception ex)
            {
                // Comprehensive error handling (requirement 12.8)
                _logger.LogError(ex, "❌ Unhandled exception in background task for job {JobId}", actualJobId);
                FailJob(actualJobId, new[] { "An unexpected error occurred during processing" });
            }
        });

        return actualJobId;
    }

    /// <summary>
    /// Gets the current status of a job
    /// </summary>
    public RepositoryJob? GetJobStatus(Guid jobId)
    {
        return JobStore.GetJob(jobId);
    }

    /// <summary>
    /// Gets the package bytes for a completed job
    /// </summary>
    public async Task<byte[]?> GetJobPackageAsync(Guid jobId)
    {
        var job = JobStore.GetJob(jobId);
        
        if (job == null || job.Status != JobStatus.Completed || string.IsNullOrEmpty(job.ResultPackagePath))
        {
            return null;
        }

        if (!File.Exists(job.ResultPackagePath))
        {
            _logger.LogWarning("⚠️ Package file not found for job {JobId}: {Path}", jobId, job.ResultPackagePath);
            return null;
        }

        return await File.ReadAllBytesAsync(job.ResultPackagePath);
    }

    /// <summary>
    /// Processes a Git import job through the full workflow
    /// Implements proper service scope management with IServiceScopeFactory (requirement 12.8)
    /// </summary>
    private async Task ProcessGitImportJobAsync(Guid jobId, string gitUrl)
    {
        // Create service scope for background task (requirement 12.8)
        using var scope = _serviceScopeFactory.CreateScope();
        var gitService = scope.ServiceProvider.GetRequiredService<IGitRepositoryService>();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        try
        {
            _logger.LogInformation("📥 Starting Git import for job {JobId}", jobId);

            // Step 1: Download repository (0-20%)
            UpdateProgress(jobId, 10, "Downloading repository from Git...");
            
            var zipBytes = await gitService.DownloadRepositoryAsync(gitUrl);
            
            _logger.LogInformation("✅ Downloaded {Size} bytes from Git for job {JobId}", zipBytes.Length, jobId);
            UpdateProgress(jobId, 20, "Repository downloaded successfully");

            // Save ZIP bytes to temp file
            var zipFilePath = await fileStorage.SaveZipBytesAsync(zipBytes);
            
            JobStore.UpdateJob(jobId, job => job.TempZipPath = zipFilePath);
            _logger.LogDebug("Saved ZIP to temp file: {Path}", zipFilePath);

            // Continue with extraction and conversion
            await ProcessRepositoryJobAsync(jobId, zipFilePath, scope);
        }
        catch (RepositoryException ex)
        {
            // User-facing errors with helpful messages
            _logger.LogWarning(ex, "⚠️ Git import job {JobId} failed with user error", jobId);
            FailJob(jobId, new[] { ex.UserMessage });
            
            // Cleanup on failure
            await CleanupTemporaryFilesAsync(jobId, fileStorage);
        }
        catch (Exception ex)
        {
            // Comprehensive error handling and logging (requirement 12.8)
            _logger.LogError(ex, "❌ Git import job {JobId} failed with unexpected error", jobId);
            FailJob(jobId, new[] { "Failed to download repository. Please check the URL and try again." });
            
            // Cleanup on failure
            await CleanupTemporaryFilesAsync(jobId, fileStorage);
        }
    }

    /// <summary>
    /// Processes a ZIP upload job through the full workflow
    /// Implements proper service scope management with IServiceScopeFactory (requirement 12.8)
    /// </summary>
    private async Task ProcessZipUploadJobAsync(Guid jobId, string zipFilePath)
    {
        // Create service scope for background task (requirement 12.8)
        using var scope = _serviceScopeFactory.CreateScope();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        
        try
        {
            _logger.LogInformation("📦 Starting ZIP upload processing for job {JobId}", jobId);
            
            await ProcessRepositoryJobAsync(jobId, zipFilePath, scope);
        }
        catch (RepositoryException ex)
        {
            // User-facing errors with helpful messages
            _logger.LogWarning(ex, "⚠️ ZIP upload job {JobId} failed with user error", jobId);
            FailJob(jobId, new[] { ex.UserMessage });
            
            // Cleanup on failure
            await CleanupTemporaryFilesAsync(jobId, fileStorage);
        }
        catch (Exception ex)
        {
            // Comprehensive error handling and logging (requirement 12.8)
            _logger.LogError(ex, "❌ ZIP upload job {JobId} failed with unexpected error", jobId);
            FailJob(jobId, new[] { "An unexpected error occurred while processing your upload." });
            
            // Cleanup on failure
            await CleanupTemporaryFilesAsync(jobId, fileStorage);
        }
    }

    /// <summary>
    /// Processes a repository job through the full workflow
    /// Uses scoped services from IServiceScopeFactory (requirement 12.8)
    /// </summary>
    private async Task ProcessRepositoryJobAsync(Guid jobId, string zipFilePath, IServiceScope scope)
    {
        // Resolve scoped services (requirement 12.8)
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        var analysisService = scope.ServiceProvider.GetRequiredService<IRepositoryAnalysisService>();
        var conversionService = scope.ServiceProvider.GetRequiredService<IRepositoryConversionService>();
        var migrationGuideService = scope.ServiceProvider.GetRequiredService<IMigrationGuideService>();
        var packagingService = scope.ServiceProvider.GetRequiredService<IPackagingService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

        string? extractPath = null;

        try
        {
            _logger.LogInformation("🔄 Processing repository job {JobId}", jobId);
            // Step 2: Extract ZIP (20-40%)
            UpdateProgress(jobId, 20, "Extracting repository files...");
            
            extractPath = await fileStorage.ExtractZipAsync(zipFilePath);
            
            JobStore.UpdateJob(jobId, job => job.TempDirectory = extractPath);
            _logger.LogInformation("✅ Extracted repository to: {Path}", extractPath);
            
            UpdateProgress(jobId, 40, "Extraction complete");

            // Step 3: Analyze repository (40-60%)
            UpdateProgress(jobId, 40, "Analyzing repository structure...");
            
            var analysisResult = await analysisService.AnalyzeRepositoryAsync(extractPath);
            
            // Check for validation errors
            if (analysisResult.ValidationErrors.Any())
            {
                _logger.LogWarning("⚠️ Repository validation failed for job {JobId}: {Errors}", 
                    jobId, string.Join(", ", analysisResult.ValidationErrors));
                FailJob(jobId, analysisResult.ValidationErrors);
                return;
            }
            
            JobStore.UpdateJob(jobId, job => job.AnalysisResult = analysisResult);
            _logger.LogInformation("✅ Analysis complete for job {JobId}: {Files} files, {LOC} LOC, {Tech} detected", 
                jobId, analysisResult.TotalFiles, analysisResult.TotalLinesOfCode, analysisResult.DetectedTechnology);
            
            UpdateProgress(jobId, 60, $"Analysis complete: {analysisResult.TotalFiles} files, {analysisResult.TotalLinesOfCode:N0} LOC");

            // Step 4: Convert files (60-80%)
            UpdateProgress(jobId, 60, "Converting files to modern technology...");
            _logger.LogInformation("🔄 Starting conversion for job {JobId}", jobId);
            
            var conversionProgress = new Progress<ConversionProgress>(progress =>
            {
                // Map conversion progress (0-100%) to job progress (60-80%)
                var jobProgress = 60 + (int)(progress.Percentage * 0.2);
                UpdateProgress(jobId, jobProgress, progress.Message);
            });

            var conversionResult = await conversionService.ConvertRepositoryAsync(
                analysisResult,
                extractPath,
                conversionProgress
            );
            
            JobStore.UpdateJob(jobId, job => job.ConversionResult = conversionResult);
            _logger.LogInformation("✅ Conversion complete for job {JobId}: {Converted} files converted, {Errors} errors, ${Cost:F2} cost", 
                jobId, conversionResult.ConvertedFiles.Count, conversionResult.Errors.Count, conversionResult.EstimatedCost);
            
            // Verify cost stays under target - Requirements 15.1, 15.2, 15.5
            const decimal MaxCostTarget = 7.0m;
            if (conversionResult.EstimatedCost > MaxCostTarget)
            {
                _logger.LogWarning("⚠️ COST ALERT: Job {JobId} exceeded target cost - ${Cost:F2} > ${Target:F2} ({ProjectSize} project with {Files} files)", 
                    jobId, conversionResult.EstimatedCost, MaxCostTarget, analysisResult.ProjectSize, conversionResult.ConvertedFiles.Count);
            }
            else
            {
                _logger.LogInformation("✅ Cost validation passed for job {JobId}: ${Cost:F2} <= ${Target:F2}", 
                    jobId, conversionResult.EstimatedCost, MaxCostTarget);
            }
            
            // Store cost data in RepositoryProject (Requirement 15.4)
            var repositoryProject = await dbContext.RepositoryProjects.FindAsync(jobId);
            if (repositoryProject != null)
            {
                repositoryProject.TotalFiles = analysisResult.TotalFiles;
                repositoryProject.TotalLinesOfCode = analysisResult.TotalLinesOfCode;
                repositoryProject.ConvertedFiles = conversionResult.ConvertedFiles.Count;
                repositoryProject.EstimatedCost = conversionResult.EstimatedCost;
                repositoryProject.SourceTechnology = analysisResult.DetectedTechnology;
                repositoryProject.ProjectSize = analysisResult.ProjectSize;
                
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("💾 Stored cost data in database: ${Cost:F2} for {Tokens} tokens", 
                    conversionResult.EstimatedCost, conversionResult.TotalTokensUsed);
            }
            
            UpdateProgress(jobId, 80, $"Conversion complete: {conversionResult.ConvertedFiles.Count} files converted");

            // Step 5: Generate migration guide for medium projects (80-85%)
            string? migrationGuide = null;
            
            if (analysisResult.ProjectSize == ProjectSize.Medium)
            {
                UpdateProgress(jobId, 80, "Generating migration guide...");
                _logger.LogInformation("📝 Generating migration guide for medium project {JobId}", jobId);
                
                migrationGuide = await migrationGuideService.GenerateMigrationGuideAsync(
                    conversionResult,
                    analysisResult
                );
                
                _logger.LogInformation("✅ Migration guide generated for job {JobId}", jobId);
                UpdateProgress(jobId, 85, "Migration guide generated");
            }

            // Step 6: Package results (85-95%)
            UpdateProgress(jobId, 85, "Packaging converted project...");
            _logger.LogInformation("📦 Creating package for job {JobId}", jobId);
            
            var projectName = Path.GetFileName(extractPath) ?? "ConvertedProject";
            var packageBytes = await packagingService.PackageRepositoryAsync(
                conversionResult,
                analysisResult,
                projectName,
                migrationGuide
            );

            // Save package to temp file
            var packagePath = Path.Combine(Path.GetTempPath(), $"{jobId}_package.zip");
            await File.WriteAllBytesAsync(packagePath, packageBytes);
            
            JobStore.UpdateJob(jobId, job => job.ResultPackagePath = packagePath);
            _logger.LogInformation("✅ Package saved to: {Path} ({Size} bytes)", packagePath, packageBytes.Length);
            
            UpdateProgress(jobId, 95, "Package created successfully");

            // Step 7: Complete job (100%)
            CompleteJob(jobId);
            
            var duration = DateTime.UtcNow - JobStore.GetJob(jobId)!.StartedAt;
            _logger.LogInformation("✅ Job {JobId} completed successfully in {Duration:mm\\:ss}", jobId, duration);
        }
        catch (RepositoryException ex)
        {
            // User-facing repository errors with helpful messages
            _logger.LogWarning(ex, "⚠️ Repository processing failed for job {JobId}: {Message}", jobId, ex.UserMessage);
            FailJob(jobId, new[] { ex.UserMessage });
        }
        catch (Exception ex)
        {
            // Comprehensive error handling and logging (requirement 12.8)
            _logger.LogError(ex, "❌ Unexpected error processing job {JobId}: {Error}", jobId, ex.Message);
            FailJob(jobId, new[] { "An unexpected error occurred during processing" });
        }
        finally
        {
            // Cleanup temporary files (requirements 14.1, 14.2, 14.3, 14.4, 14.5)
            await CleanupTemporaryFilesAsync(jobId, fileStorage);
        }
    }

    /// <summary>
    /// Updates job progress and status message
    /// </summary>
    private void UpdateProgress(Guid jobId, int percentage, string message)
    {
        JobStore.UpdateJob(jobId, job =>
        {
            job.ProgressPercentage = percentage;
            job.StatusMessage = message;
            
            // Update status based on percentage
            job.Status = percentage switch
            {
                < 20 => JobStatus.Downloading,
                < 40 => JobStatus.Extracting,
                < 60 => JobStatus.Analyzing,
                < 80 => JobStatus.Converting,
                < 100 => JobStatus.Packaging,
                _ => job.Status
            };
        });

        _logger.LogDebug("Job {JobId} progress: {Percentage}% - {Message}", jobId, percentage, message);
    }

    /// <summary>
    /// Marks a job as completed
    /// </summary>
    private void CompleteJob(Guid jobId)
    {
        JobStore.UpdateJob(jobId, job =>
        {
            job.Status = JobStatus.Completed;
            job.ProgressPercentage = 100;
            job.StatusMessage = "Conversion completed successfully! 🎉";
            job.CompletedAt = DateTime.UtcNow;
        });

        _logger.LogInformation("✅ Job {JobId} marked as completed", jobId);
    }

    /// <summary>
    /// Marks a job as failed with error messages
    /// </summary>
    private void FailJob(Guid jobId, IEnumerable<string> errors)
    {
        JobStore.UpdateJob(jobId, job =>
        {
            job.Status = JobStatus.Failed;
            job.StatusMessage = "Conversion failed";
            job.Errors = errors.ToList();
            job.CompletedAt = DateTime.UtcNow;
        });

        _logger.LogWarning("❌ Job {JobId} marked as failed: {Errors}", jobId, string.Join(", ", errors));
    }

    /// <summary>
    /// Cleans up temporary files after job completion or failure
    /// Requirements: 14.1, 14.2, 14.3, 14.4, 14.5
    /// </summary>
    private async Task CleanupTemporaryFilesAsync(Guid jobId, IFileStorageService fileStorage)
    {
        var job = JobStore.GetJob(jobId);
        if (job == null)
        {
            _logger.LogWarning("⚠️ Cannot cleanup job {JobId}: job not found", jobId);
            return;
        }

        _logger.LogInformation("🧹 Starting cleanup for job {JobId}", jobId);

        try
        {
            // Delete uploaded ZIP file (requirement 14.1)
            if (!string.IsNullOrEmpty(job.TempZipPath) && File.Exists(job.TempZipPath))
            {
                try
                {
                    await fileStorage.DeleteFileAsync(job.TempZipPath);
                    _logger.LogDebug("Deleted temp ZIP: {Path}", job.TempZipPath);
                }
                catch (Exception ex)
                {
                    // Handle cleanup errors gracefully (requirement 14.5)
                    _logger.LogWarning(ex, "⚠️ Failed to delete temp ZIP file: {Path}", job.TempZipPath);
                }
            }

            // Remove extracted directories (requirement 14.2)
            if (!string.IsNullOrEmpty(job.TempDirectory) && Directory.Exists(job.TempDirectory))
            {
                try
                {
                    await fileStorage.DeleteDirectoryAsync(job.TempDirectory);
                    _logger.LogDebug("Deleted temp directory: {Path}", job.TempDirectory);
                }
                catch (Exception ex)
                {
                    // Handle cleanup errors gracefully (requirement 14.5)
                    _logger.LogWarning(ex, "⚠️ Failed to delete temp directory: {Path}", job.TempDirectory);
                }
            }

            // Clean up intermediate conversion artifacts (requirement 14.3)
            // Note: Intermediate artifacts are stored in the temp directory, so they're already cleaned up above

            // Retain final packaged ZIP for download (requirement 14.4)
            // The ResultPackagePath is intentionally NOT deleted here
            if (!string.IsNullOrEmpty(job.ResultPackagePath))
            {
                _logger.LogDebug("Retained final package: {Path}", job.ResultPackagePath);
            }

            _logger.LogInformation("✅ Cleanup completed for job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            // Handle cleanup errors gracefully - don't fail the job (requirement 14.5)
            _logger.LogError(ex, "❌ Unexpected error during cleanup for job {JobId}", jobId);
        }
    }
}
