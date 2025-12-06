using Microsoft.AspNetCore.Mvc;
using StackOverGrave.Api.Data;
using StackOverGrave.Api.Models;
using StackOverGrave.Api.Services;

namespace StackOverGrave.Api.Controllers;

[ApiController]
[Route("api/repository")]
public class RepositoryController : ControllerBase
{
    private readonly ILogger<RepositoryController> _logger;
    private readonly IRepositoryJobProcessor _jobProcessor;
    private readonly IFileStorageService _fileStorage;
    private readonly AppDbContext _context;

    public RepositoryController(
        ILogger<RepositoryController> logger,
        IRepositoryJobProcessor jobProcessor,
        IFileStorageService fileStorage,
        AppDbContext context)
    {
        _logger = logger;
        _jobProcessor = jobProcessor;
        _fileStorage = fileStorage;
        _context = context;
    }

    /// <summary>
    /// Import a repository from a Git URL (GitHub, GitLab, Bitbucket)
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportFromGit([FromBody] GitImportRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest(new 
                { 
                    error = "Git URL is required",
                    details = new { field = "url" },
                    suggestion = "Provide a valid GitHub, GitLab, or Bitbucket repository URL"
                });
            }

            // Validate Git URL format
            if (!IsValidGitUrl(request.Url))
            {
                return BadRequest(new 
                { 
                    error = "Invalid Git URL format",
                    details = new { url = request.Url },
                    suggestion = "URL must be from GitHub, GitLab, or Bitbucket (e.g., https://github.com/user/repo)"
                });
            }

            _logger.LogInformation("📥 Starting Git import from: {Url}", request.Url);

            // Start background job for Git download first to get the job ID
            var jobId = await _jobProcessor.StartGitImportJobAsync(request.Url);

            // Create RepositoryProject record with the same ID as the job
            var repositoryProject = new RepositoryProject
            {
                Id = jobId,
                Name = ExtractRepoName(request.Url),
                Source = RepositorySource.Git,
                SourceUrl = request.Url,
                UploadedAt = DateTime.UtcNow,
                OriginalFilename = $"{ExtractRepoName(request.Url)}.zip"
            };

            _context.RepositoryProjects.Add(repositoryProject);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Git import job started: {JobId} for repository: {RepoId}", 
                jobId, repositoryProject.Id);

            return Ok(new 
            { 
                repository_id = jobId,
                status = "downloading",
                message = "Repository import started"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting Git import from: {Url}", request.Url);
            return StatusCode(500, new 
            { 
                error = "Failed to start repository import",
                suggestion = "Please try again or check if the repository is accessible"
            });
        }
    }

    /// <summary>
    /// Upload a ZIP file containing a repository
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadZip(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new 
                { 
                    error = "No file provided",
                    suggestion = "Please select a ZIP file to upload"
                });
            }

            // Validate file size (50MB limit)
            const long maxFileSize = 50 * 1024 * 1024; // 50MB
            if (file.Length > maxFileSize)
            {
                return BadRequest(new 
                { 
                    error = "File size exceeds limit",
                    details = new 
                    { 
                        actualSize = file.Length,
                        maxSize = maxFileSize,
                        actualSizeMB = Math.Round(file.Length / (1024.0 * 1024.0), 2),
                        maxSizeMB = 50
                    },
                    suggestion = "Please upload a ZIP file smaller than 50MB"
                });
            }

            // Validate ZIP file format
            if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new 
                { 
                    error = "Invalid file format",
                    details = new { filename = file.FileName },
                    suggestion = "Only ZIP files are supported"
                });
            }

            _logger.LogInformation("📤 Starting ZIP upload: {Filename} ({Size} bytes)", 
                file.FileName, file.Length);

            // Generate a job ID first
            var jobId = Guid.NewGuid();

            // Create RepositoryProject record with the same ID as the job
            var repositoryProject = new RepositoryProject
            {
                Id = jobId,
                Name = Path.GetFileNameWithoutExtension(file.FileName),
                Source = RepositorySource.Upload,
                OriginalFilename = file.FileName,
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            _context.RepositoryProjects.Add(repositoryProject);
            await _context.SaveChangesAsync();

            // Save ZIP file to temporary location
            var tempZipPath = await _fileStorage.SaveFileAsync(file, repositoryProject.Id);

            // Start background job for ZIP extraction and processing
            await _jobProcessor.StartZipUploadJobAsync(tempZipPath, jobId);

            _logger.LogInformation("✅ ZIP upload job started: {JobId} for repository: {RepoId}", 
                jobId, repositoryProject.Id);

            return Ok(new 
            { 
                repository_id = jobId,
                status = "extracting",
                message = "ZIP file uploaded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error uploading ZIP file");
            return StatusCode(500, new 
            { 
                error = "Failed to upload ZIP file",
                suggestion = "Please try again or check if the file is a valid ZIP archive"
            });
        }
    }

    /// <summary>
    /// Get the status of a repository conversion job
    /// </summary>
    [HttpGet("status/{id}")]
    public IActionResult GetStatus(Guid id)
    {
        try
        {
            var job = _jobProcessor.GetJobStatus(id);

            if (job == null)
            {
                return NotFound(new 
                { 
                    error = "Job not found",
                    details = new { jobId = id },
                    suggestion = "Please check the job ID and try again"
                });
            }

            _logger.LogInformation("📊 Status check for job: {JobId} - {Status} ({Progress}%)", 
                id, job.Status, job.ProgressPercentage);

            return Ok(new
            {
                repository_id = job.Id,
                status = job.Status.ToString().ToLower(),
                progress = job.ProgressPercentage,
                message = job.StatusMessage,
                started_at = job.StartedAt,
                completed_at = job.CompletedAt,
                analysis_result = job.AnalysisResult,
                conversion_result = job.ConversionResult,
                errors = job.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting status for job: {JobId}", id);
            return StatusCode(500, new 
            { 
                error = "Failed to get job status",
                suggestion = "Please try again"
            });
        }
    }

    /// <summary>
    /// Download the converted repository package
    /// </summary>
    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadPackage(Guid id)
    {
        try
        {
            var job = _jobProcessor.GetJobStatus(id);

            if (job == null)
            {
                return NotFound(new 
                { 
                    error = "Job not found",
                    details = new { jobId = id },
                    suggestion = "Please check the job ID and try again"
                });
            }

            if (job.Status != JobStatus.Completed)
            {
                return BadRequest(new 
                { 
                    error = "Conversion not completed",
                    details = new 
                    { 
                        currentStatus = job.Status.ToString(),
                        progress = job.ProgressPercentage
                    },
                    suggestion = job.Status == JobStatus.Failed 
                        ? "The conversion failed. Please check the errors and try again."
                        : "Please wait for the conversion to complete before downloading."
                });
            }

            var packageBytes = await _jobProcessor.GetJobPackageAsync(id);

            if (packageBytes == null || packageBytes.Length == 0)
            {
                return NotFound(new 
                { 
                    error = "Package not found",
                    details = new { jobId = id },
                    suggestion = "The package may have been deleted or is no longer available"
                });
            }

            var filename = job.AnalysisResult?.DetectedTechnology != TechnologyType.Unknown
                ? $"{ExtractProjectName(job)}-Resurrected.zip"
                : $"project-{id}-resurrected.zip";

            _logger.LogInformation("📦 Downloading package for job: {JobId} ({Size} bytes)", 
                id, packageBytes.Length);

            return File(packageBytes, "application/zip", filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error downloading package for job: {JobId}", id);
            return StatusCode(500, new 
            { 
                error = "Failed to download package",
                suggestion = "Please try again"
            });
        }
    }

    #region Helper Methods

    private bool IsValidGitUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        try
        {
            var uri = new Uri(url);
            var allowedDomains = new[] { "github.com", "gitlab.com", "bitbucket.org" };
            return allowedDomains.Any(domain => uri.Host.Equals(domain, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    private string ExtractRepoName(string url)
    {
        try
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Trim('/').Split('/');
            if (segments.Length >= 2)
            {
                var repoName = segments[^1].Replace(".git", "");
                return repoName;
            }
            return "repository";
        }
        catch
        {
            return "repository";
        }
    }

    private string ExtractProjectName(RepositoryJob job)
    {
        // Try to get a meaningful project name from the job
        if (job.TempDirectory != null)
        {
            var dirName = Path.GetFileName(job.TempDirectory);
            if (!string.IsNullOrWhiteSpace(dirName))
                return dirName;
        }
        return "project";
    }

    #endregion
}
