using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackOverGrave.Api.Data;
using StackOverGrave.Api.Models;
using StackOverGrave.Api.Services;

namespace StackOverGrave.Api.Controllers;

[ApiController]
[Route("api")]
public class ProjectController : ControllerBase
{
    private readonly ILogger<ProjectController> _logger;
    private readonly IFileStorageService _fileStorage;
    private readonly ITechnologyDetectionService _techDetection;
    private readonly IAiConversionService _aiConversion;
    private readonly IPackagingService _packaging;
    private readonly AppDbContext _context;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ProjectController(
        ILogger<ProjectController> logger,
        IFileStorageService fileStorage,
        ITechnologyDetectionService techDetection,
        IAiConversionService aiConversion,
        IPackagingService packaging,
        AppDbContext context,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _fileStorage = fileStorage;
        _techDetection = techDetection;
        _aiConversion = aiConversion;
        _packaging = packaging;
        _context = context;
        _serviceScopeFactory = serviceScopeFactory;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided" });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { error = "File size exceeds 5MB limit" });

            var project = new Project
            {
                Id = Guid.NewGuid(),
                OriginalFilename = file.FileName,
                UploadedAt = DateTime.UtcNow,
                Status = ProjectStatus.Uploaded,
                FileSize = file.Length
            };

            var filePath = await _fileStorage.SaveFileAsync(file, project.Id);
            project.FilePath = filePath;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("File uploaded: {Filename} ({Id})", file.FileName, project.Id);

            return Ok(new { id = project.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { error = "Upload failed" });
        }
    }

    [HttpGet("analyze/{id}")]
    public async Task<IActionResult> Analyze(Guid id)
    {
        try
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { error = "Project not found" });

            var certificate = await _techDetection.AnalyzeFileAsync(project.FilePath);
            
            project.Technology = certificate.Technology;
            project.LinesOfCode = certificate.FileStats.LinesOfCode;
            await _context.SaveChangesAsync();

            _logger.LogInformation("File analyzed: {Id} - {Tech}", id, certificate.Technology);

            return Ok(certificate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing file {Id}", id);
            return StatusCode(500, new { error = "Analysis failed" });
        }
    }

    [HttpPost("resurrect/{id}")]
    public async Task<IActionResult> Resurrect(Guid id)
    {
        try
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { error = "Project not found" });

            project.Status = ProjectStatus.Processing;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Resurrection started: {Id}", id);

            // Start conversion in background using service scope factory
            _ = Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("🚀 Background task starting for {Id}", id);
                    await ProcessConversionAsync(id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Unhandled exception in background task for project {Id}", id);
                }
            });

            return Accepted(new { message = "Resurrection in progress" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting resurrection {Id}", id);
            return StatusCode(500, new { error = "Resurrection failed to start" });
        }
    }

    [HttpGet("status/{id}")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        try
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { error = "Project not found" });

            return Ok(new
            {
                id = project.Id,
                status = project.Status.ToString(),
                technology = project.Technology.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status {Id}", id);
            return StatusCode(500, new { error = "Failed to get status" });
        }
    }

    [HttpGet("result/{id}")]
    public async Task<IActionResult> GetResult(Guid id)
    {
        try
        {
            var result = await _context.ConversionResults
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.ProjectId == id);

            if (result == null)
                return NotFound(new { error = "Result not found" });

            var migrationData = System.Text.Json.JsonSerializer.Deserialize<MigrationData>(result.MigrationNotes);

            return Ok(new
            {
                originalCode = result.OriginalCode,
                convertedCode = result.ConvertedCode,
                originalFilename = result.Project?.OriginalFilename,
                convertedFilename = GetConvertedFilename(result.Project?.OriginalFilename ?? "", result.Project?.Technology ?? TechnologyType.Unknown),
                sourceTech = result.Project?.Technology.ToString(),
                targetTech = GetTargetTech(result.Project?.Technology ?? TechnologyType.Unknown),
                migrationNotes = migrationData?.MigrationNotes ?? new List<string>(),
                dependencies = migrationData?.Dependencies ?? new List<string>(),
                breakingChanges = migrationData?.BreakingChanges ?? new List<string>(),
                warnings = migrationData?.Warnings ?? new List<string>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting result {Id}", id);
            return StatusCode(500, new { error = "Failed to get result" });
        }
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(Guid id)
    {
        try
        {
            var result = await _context.ConversionResults
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.ProjectId == id);

            if (result == null)
                return NotFound(new { error = "Result not found" });

            var zipBytes = await _packaging.GenerateZipAsync(result);
            var filename = $"{Path.GetFileNameWithoutExtension(result.Project?.OriginalFilename)}_converted.zip";

            return File(zipBytes, "application/zip", filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading {Id}", id);
            return StatusCode(500, new { error = "Download failed" });
        }
    }

    [HttpGet("graveyard")]
    public async Task<IActionResult> GetGraveyard()
    {
        try
        {
            var projects = await _context.Projects
                .OrderByDescending(p => p.UploadedAt)
                .Take(100)
                .ToListAsync();

            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting graveyard");
            return StatusCode(500, new { error = "Failed to get projects" });
        }
    }

    private async Task ProcessConversionAsync(Guid projectId)
    {
        // Create a new scope for the background task using the factory
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var aiConversion = scope.ServiceProvider.GetRequiredService<IAiConversionService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ProjectController>>();
        
        try
        {
            logger.LogInformation("🔄 Starting conversion for project {Id}", projectId);
            
            var project = await context.Projects.FindAsync(projectId);
            if (project == null)
            {
                logger.LogError("❌ Project not found: {Id}", projectId);
                return;
            }
            
            logger.LogInformation("📂 File path: {Path}", project.FilePath);
            
            if (!System.IO.File.Exists(project.FilePath))
            {
                logger.LogError("❌ File not found at path: {Path}", project.FilePath);
                project.Status = ProjectStatus.Failed;
                await context.SaveChangesAsync();
                return;
            }
            
            var originalCode = await System.IO.File.ReadAllTextAsync(project.FilePath);
            logger.LogInformation("📄 Read {Length} characters from file", originalCode.Length);
            
            var targetTech = GetTargetTechType(project.Technology);
            logger.LogInformation("🎯 Converting {Source} to {Target}", project.Technology, targetTech);

            logger.LogInformation("🤖 Calling OpenAI API...");
            var conversionResult = await aiConversion.ConvertCodeAsync(
                originalCode,
                project.Technology,
                targetTech
            );
            logger.LogInformation("✅ OpenAI conversion completed");

            var result = new ConversionResult
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                OriginalCode = originalCode,
                ConvertedCode = conversionResult.ConvertedCode,
                MigrationNotes = System.Text.Json.JsonSerializer.Serialize(new MigrationData
                {
                    MigrationNotes = conversionResult.MigrationNotes,
                    Dependencies = conversionResult.Dependencies,
                    BreakingChanges = conversionResult.BreakingChanges,
                    Warnings = conversionResult.Warnings
                }),
                ProcessedAt = DateTime.UtcNow
            };

            context.ConversionResults.Add(result);
            project.Status = ProjectStatus.Completed;
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Conversion completed successfully: {Id}", project.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Conversion failed: {Id}", projectId);
            
            try
            {
                var project = await context.Projects.FindAsync(projectId);
                if (project != null)
                {
                    project.Status = ProjectStatus.Failed;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception saveEx)
            {
                logger.LogError(saveEx, "❌ Failed to update project status to Failed");
            }
        }
    }

    private TechnologyType GetTargetTechType(TechnologyType source) => source switch
    {
        TechnologyType.VB6 => TechnologyType.Unknown, // Will be C# .NET 8
        TechnologyType.ActionScript => TechnologyType.Unknown, // Will be TypeScript
        TechnologyType.Silverlight => TechnologyType.Unknown, // Will be Angular
        TechnologyType.DotNetFramework => TechnologyType.Unknown, // Will be .NET 8
        _ => TechnologyType.Unknown
    };

    private string GetTargetTech(TechnologyType source) => source switch
    {
        TechnologyType.VB6 => "C# .NET 8",
        TechnologyType.ActionScript => "TypeScript/Angular",
        TechnologyType.Silverlight => "Angular Material",
        TechnologyType.DotNetFramework => ".NET 8",
        _ => "Modern Framework"
    };

    private string GetConvertedFilename(string original, TechnologyType tech)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(original);
        return tech switch
        {
            TechnologyType.VB6 => $"{nameWithoutExt}.cs",
            TechnologyType.ActionScript => $"{nameWithoutExt}.component.ts",
            TechnologyType.Silverlight => $"{nameWithoutExt}.component.ts",
            TechnologyType.DotNetFramework => $"{nameWithoutExt}.cs",
            _ => $"{nameWithoutExt}.txt"
        };
    }
}
