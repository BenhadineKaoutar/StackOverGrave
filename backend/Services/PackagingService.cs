using System.IO.Compression;
using System.Text;
using StackOverGrave.Api.Models;

namespace StackOverGrave.Api.Services;

public class PackagingService : IPackagingService
{
    private readonly ILogger<PackagingService> _logger;

    public PackagingService(ILogger<PackagingService> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> GenerateZipAsync(ConversionResult result)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var tech = result.Project?.Technology ?? TechnologyType.Unknown;
            
            // Add converted code file
            var codeFileName = GetCodeFileName(result.Project?.OriginalFilename ?? "converted", tech);
            AddFileToZip(archive, $"src/{codeFileName}", result.ConvertedCode);

            // Add README
            var readme = GenerateReadme(result);
            AddFileToZip(archive, "README.md", readme);

            // Add MIGRATION_NOTES
            var migrationNotes = GenerateMigrationNotes(result);
            AddFileToZip(archive, "MIGRATION_NOTES.md", migrationNotes);

            // Add project file based on technology
            var projectFile = GenerateProjectFile(result);
            if (!string.IsNullOrEmpty(projectFile.content))
            {
                AddFileToZip(archive, projectFile.filename, projectFile.content);
            }
        }

        memoryStream.Position = 0;
        return Task.FromResult(memoryStream.ToArray());
    }

    public Task<byte[]> PackageRepositoryAsync(
        RepositoryConversionResult conversionResult,
        RepositoryAnalysisResult analysisResult,
        string projectName,
        string? migrationGuide = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📦 Packaging repository: {ProjectName}", projectName);

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var packageName = $"{projectName}-Resurrected";
            
            // Organize converted files into appropriate directories
            foreach (var file in conversionResult.ConvertedFiles)
            {
                var targetPath = DetermineTargetDirectory(file.ConvertedPath);
                var entryPath = $"{packageName}/{targetPath}";
                
                _logger.LogDebug("Adding file: {Path}", entryPath);
                AddFileToZip(archive, entryPath, file.ConvertedCode);
            }

            // Add README.md
            var readme = GenerateRepositoryReadme(analysisResult, conversionResult, projectName);
            AddFileToZip(archive, $"{packageName}/README.md", readme);

            // Add MIGRATION_GUIDE.md for medium projects
            if (analysisResult.ProjectSize == ProjectSize.Medium && !string.IsNullOrEmpty(migrationGuide))
            {
                _logger.LogInformation("📝 Including migration guide for medium project");
                AddFileToZip(archive, $"{packageName}/MIGRATION_GUIDE.md", migrationGuide);
            }

            // Add .gitignore
            var gitignore = GenerateGitignore(analysisResult.DetectedTechnology);
            AddFileToZip(archive, $"{packageName}/.gitignore", gitignore);

            // Add project configuration file (.csproj or package.json)
            var projectFile = GenerateRepositoryProjectFile(
                analysisResult.DetectedTechnology,
                projectName,
                conversionResult
            );
            
            if (!string.IsNullOrEmpty(projectFile.content))
            {
                AddFileToZip(archive, $"{packageName}/{projectFile.filename}", projectFile.content);
            }

            _logger.LogInformation("✅ Package created successfully: {PackageName}.zip", packageName);
        }

        memoryStream.Position = 0;
        return Task.FromResult(memoryStream.ToArray());
    }

    private void AddFileToZip(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName);
        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream, Encoding.UTF8);
        writer.Write(content);
    }

    private string GetCodeFileName(string originalName, TechnologyType tech)
    {
        var baseName = Path.GetFileNameWithoutExtension(originalName);
        return tech switch
        {
            TechnologyType.VB6 => $"{baseName}.cs",
            TechnologyType.ActionScript => $"{baseName}.component.ts",
            TechnologyType.Silverlight => $"{baseName}.component.ts",
            TechnologyType.DotNetFramework => $"{baseName}.cs",
            _ => $"{baseName}.txt"
        };
    }

    private string GenerateReadme(ConversionResult result)
    {
        var tech = result.Project?.Technology ?? TechnologyType.Unknown;
        var targetTech = GetTargetTech(tech);
        var originalName = result.Project?.OriginalFilename ?? "Unknown";

        var sb = new StringBuilder();
        sb.AppendLine($"# Converted Project: {Path.GetFileNameWithoutExtension(originalName)}");
        sb.AppendLine();
        sb.AppendLine("## Original Technology");
        sb.AppendLine($"{tech}");
        sb.AppendLine();
        sb.AppendLine("## New Technology");
        sb.AppendLine($"{targetTech}");
        sb.AppendLine();
        sb.AppendLine("## Quick Start");
        sb.AppendLine();
        sb.AppendLine(GetQuickStartInstructions(tech));
        sb.AppendLine();
        sb.AppendLine("## Files Included");
        sb.AppendLine();
        sb.AppendLine("- `src/` - Converted source code");
        sb.AppendLine("- `README.md` - This file");
        sb.AppendLine("- `MIGRATION_NOTES.md` - Detailed migration information");
        sb.AppendLine();
        sb.AppendLine("## Next Steps");
        sb.AppendLine();
        sb.AppendLine("1. Review the converted code");
        sb.AppendLine("2. Install dependencies (see MIGRATION_NOTES.md)");
        sb.AppendLine("3. Address breaking changes");
        sb.AppendLine("4. Test thoroughly");
        sb.AppendLine("5. Deploy!");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine("*Generated by StackOverGrave - Where Legacy Code Rests in Peace*");

        return sb.ToString();
    }

    private string GenerateMigrationNotes(ConversionResult result)
    {
        var migrationData = System.Text.Json.JsonSerializer.Deserialize<MigrationData>(result.MigrationNotes);
        
        var sb = new StringBuilder();
        sb.AppendLine("# Migration Notes");
        sb.AppendLine();
        sb.AppendLine($"**Processed:** {result.ProcessedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        if (migrationData?.MigrationNotes?.Any() == true)
        {
            sb.AppendLine("## Changes Made");
            sb.AppendLine();
            foreach (var note in migrationData.MigrationNotes)
            {
                sb.AppendLine($"- {note}");
            }
            sb.AppendLine();
        }

        if (migrationData?.Dependencies?.Any() == true)
        {
            sb.AppendLine("## Dependencies Required");
            sb.AppendLine();
            foreach (var dep in migrationData.Dependencies)
            {
                sb.AppendLine($"- `{dep}`");
            }
            sb.AppendLine();
            sb.AppendLine("Install with:");
            sb.AppendLine("```bash");
            sb.AppendLine(GetInstallCommand(result.Project?.Technology ?? TechnologyType.Unknown, migrationData.Dependencies));
            sb.AppendLine("```");
            sb.AppendLine();
        }

        if (migrationData?.BreakingChanges?.Any() == true)
        {
            sb.AppendLine("## ⚠️ Breaking Changes");
            sb.AppendLine();
            foreach (var change in migrationData.BreakingChanges)
            {
                sb.AppendLine($"- {change}");
            }
            sb.AppendLine();
        }

        if (migrationData?.Warnings?.Any() == true)
        {
            sb.AppendLine("## 🚨 Warnings");
            sb.AppendLine();
            foreach (var warning in migrationData.Warnings)
            {
                sb.AppendLine($"- {warning}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private (string filename, string content) GenerateProjectFile(ConversionResult result)
    {
        var tech = result.Project?.Technology ?? TechnologyType.Unknown;
        
        return tech switch
        {
            TechnologyType.VB6 or TechnologyType.DotNetFramework => GenerateCsProjFile(result),
            TechnologyType.ActionScript or TechnologyType.Silverlight => GeneratePackageJson(result),
            _ => ("", "")
        };
    }

    private (string filename, string content) GenerateCsProjFile(ConversionResult result)
    {
        var projectName = Path.GetFileNameWithoutExtension(result.Project?.OriginalFilename ?? "ConvertedProject");
        
        var content = $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.Extensions.Logging"" Version=""8.0.0"" />
    <PackageReference Include=""Microsoft.Extensions.DependencyInjection"" Version=""8.0.0"" />
  </ItemGroup>

</Project>";

        return ($"{projectName}.csproj", content);
    }

    private (string filename, string content) GeneratePackageJson(ConversionResult result)
    {
        var projectName = Path.GetFileNameWithoutExtension(result.Project?.OriginalFilename ?? "converted-project");
        
        var content = $@"{{
  ""name"": ""{projectName.ToLower()}"",
  ""version"": ""1.0.0"",
  ""scripts"": {{
    ""start"": ""ng serve"",
    ""build"": ""ng build"",
    ""test"": ""ng test""
  }},
  ""dependencies"": {{
    ""@angular/core"": ""^17.0.0"",
    ""@angular/common"": ""^17.0.0"",
    ""@angular/material"": ""^17.0.0"",
    ""rxjs"": ""^7.8.0""
  }}
}}";

        return ("package.json", content);
    }

    private string GetTargetTech(TechnologyType tech) => tech switch
    {
        TechnologyType.VB6 => "C# .NET 8",
        TechnologyType.ActionScript => "TypeScript/Angular",
        TechnologyType.Silverlight => "Angular Material",
        TechnologyType.DotNetFramework => ".NET 8",
        _ => "Modern Framework"
    };

    private string GetQuickStartInstructions(TechnologyType tech) => tech switch
    {
        TechnologyType.VB6 or TechnologyType.DotNetFramework => @"```bash
dotnet restore
dotnet build
dotnet run
```",
        TechnologyType.ActionScript or TechnologyType.Silverlight => @"```bash
npm install
npm start
```",
        _ => "See documentation for setup instructions."
    };

    private string GetInstallCommand(TechnologyType tech, List<string> dependencies)
    {
        return tech switch
        {
            TechnologyType.VB6 or TechnologyType.DotNetFramework => 
                string.Join("\n", dependencies.Select(d => $"dotnet add package {d}")),
            TechnologyType.ActionScript or TechnologyType.Silverlight => 
                $"npm install {string.Join(" ", dependencies)}",
            _ => "# Install dependencies manually"
        };
    }

    /// <summary>
    /// Determines the target directory structure for a converted file
    /// </summary>
    private string DetermineTargetDirectory(string convertedPath)
    {
        var fileName = Path.GetFileName(convertedPath);
        var directory = Path.GetDirectoryName(convertedPath)?.Replace("\\", "/") ?? "";
        
        // Normalize directory names
        var lowerDir = directory.ToLowerInvariant();
        
        if (lowerDir.Contains("models") || lowerDir.Contains("entities"))
        {
            return $"src/Models/{fileName}";
        }
        else if (lowerDir.Contains("services") || lowerDir.Contains("business"))
        {
            return $"src/Services/{fileName}";
        }
        else if (lowerDir.Contains("controllers") || lowerDir.Contains("api"))
        {
            return $"src/Controllers/{fileName}";
        }
        else if (lowerDir.Contains("views") || lowerDir.Contains("forms") || lowerDir.Contains("components"))
        {
            return $"src/Components/{fileName}";
        }
        else
        {
            // Keep original structure under src/
            return string.IsNullOrEmpty(directory) 
                ? $"src/{fileName}" 
                : $"src/{directory}/{fileName}";
        }
    }

    /// <summary>
    /// Generates README.md for repository package
    /// </summary>
    private string GenerateRepositoryReadme(
        RepositoryAnalysisResult analysisResult,
        RepositoryConversionResult conversionResult,
        string projectName)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"# {projectName} - Resurrected");
        sb.AppendLine();
        sb.AppendLine("🎃 **This project has been resurrected from legacy code by StackOverGrave**");
        sb.AppendLine();
        
        sb.AppendLine("## Project Information");
        sb.AppendLine();
        sb.AppendLine($"- **Original Technology**: {GetTechnologyDisplayName(analysisResult.DetectedTechnology)}");
        sb.AppendLine($"- **Target Technology**: {GetTargetTechnologyDisplayName(analysisResult.DetectedTechnology)}");
        sb.AppendLine($"- **Total Files Analyzed**: {analysisResult.TotalFiles}");
        sb.AppendLine($"- **Files Converted**: {conversionResult.ConvertedFiles.Count}");
        sb.AppendLine($"- **Total Lines of Code**: {analysisResult.TotalLinesOfCode:N0}");
        sb.AppendLine($"- **Conversion Cost**: ${conversionResult.EstimatedCost:F2}");
        sb.AppendLine();
        
        sb.AppendLine("## Project Structure");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine($"{projectName}-Resurrected/");
        sb.AppendLine("├── src/");
        sb.AppendLine("│   ├── Models/       # Data models and entities");
        sb.AppendLine("│   ├── Services/     # Business logic and services");
        sb.AppendLine("│   ├── Controllers/  # API controllers or UI controllers");
        sb.AppendLine("│   └── Components/   # UI components");
        sb.AppendLine("├── README.md         # This file");
        
        if (analysisResult.ProjectSize == ProjectSize.Medium)
        {
            sb.AppendLine("├── MIGRATION_GUIDE.md  # Detailed migration instructions");
        }
        
        sb.AppendLine("└── [project file]    # .csproj or package.json");
        sb.AppendLine("```");
        sb.AppendLine();
        
        sb.AppendLine("## Getting Started");
        sb.AppendLine();
        sb.AppendLine(GetSetupInstructions(analysisResult.DetectedTechnology));
        sb.AppendLine();
        
        if (analysisResult.ProjectSize == ProjectSize.Medium)
        {
            sb.AppendLine("## Important Notes");
            sb.AppendLine();
            sb.AppendLine($"⚠️ This is a **medium-sized project**. Only the top {conversionResult.ConvertedFiles.Count} most critical files have been converted.");
            sb.AppendLine();
            sb.AppendLine("**Please review the MIGRATION_GUIDE.md file** for:");
            sb.AppendLine("- Detailed conversion notes for each file");
            sb.AppendLine("- Manual steps required to complete the migration");
            sb.AppendLine("- Breaking changes and warnings");
            sb.AppendLine("- Find-and-replace patterns for remaining files");
            sb.AppendLine();
        }
        
        sb.AppendLine("## Next Steps");
        sb.AppendLine();
        sb.AppendLine("1. **Review the converted code** - Check that business logic is preserved");
        sb.AppendLine("2. **Install dependencies** - Run the appropriate package manager commands");
        sb.AppendLine("3. **Update configuration** - Update connection strings, API keys, etc.");
        sb.AppendLine("4. **Run tests** - Ensure functionality works as expected");
        
        if (analysisResult.ProjectSize == ProjectSize.Medium)
        {
            sb.AppendLine("5. **Complete remaining conversions** - Use the migration guide to convert remaining files");
        }
        
        sb.AppendLine();
        sb.AppendLine("## Converted Files");
        sb.AppendLine();
        
        foreach (var file in conversionResult.ConvertedFiles)
        {
            sb.AppendLine($"- `{file.OriginalPath}` → `{file.ConvertedPath}`");
        }
        
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("*Generated by [StackOverGrave](https://github.com/BenhadineKaoutar/stackovergrave) - Where Legacy Code Rests in Peace* 🪦");
        
        return sb.ToString();
    }

    /// <summary>
    /// Generates .gitignore file appropriate to target technology
    /// </summary>
    private string GenerateGitignore(TechnologyType tech)
    {
        return tech switch
        {
            TechnologyType.VB6 or TechnologyType.DotNetFramework => @"# .NET
bin/
obj/
*.user
*.suo
.vs/
*.cache
*.dll
*.exe
*.pdb
*.log

# Build results
[Dd]ebug/
[Rr]elease/
x64/
x86/
[Aa]rtifacts/

# NuGet
*.nupkg
packages/
.nuget/

# IDE
.vscode/
.idea/
*.swp
*~
",

            TechnologyType.ActionScript or TechnologyType.Silverlight => @"# Node
node_modules/
npm-debug.log*
yarn-debug.log*
yarn-error.log*

# Angular
dist/
.angular/
.sass-cache/

# IDE
.vscode/
.idea/
*.swp
*~

# OS
.DS_Store
Thumbs.db

# Environment
.env
.env.local
",

            _ => @"# Build artifacts
bin/
obj/
dist/
node_modules/

# IDE
.vscode/
.idea/
*.swp

# OS
.DS_Store
Thumbs.db
"
        };
    }

    /// <summary>
    /// Generates project configuration file for repository
    /// </summary>
    private (string filename, string content) GenerateRepositoryProjectFile(
        TechnologyType sourceTech,
        string projectName,
        RepositoryConversionResult conversionResult)
    {
        return sourceTech switch
        {
            TechnologyType.VB6 or TechnologyType.DotNetFramework => 
                GenerateRepositoryCsProjFile(projectName, conversionResult),
            TechnologyType.ActionScript or TechnologyType.Silverlight => 
                GenerateRepositoryPackageJson(projectName, conversionResult),
            _ => ("", "")
        };
    }

    /// <summary>
    /// Generates .csproj file for .NET projects with dependencies
    /// </summary>
    private (string filename, string content) GenerateRepositoryCsProjFile(
        string projectName,
        RepositoryConversionResult conversionResult)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        sb.AppendLine();
        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine("    <TargetFramework>net8.0</TargetFramework>");
        sb.AppendLine("    <Nullable>enable</Nullable>");
        sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
        sb.AppendLine("    <RootNamespace>" + projectName.Replace("-", "_") + "</RootNamespace>");
        sb.AppendLine("  </PropertyGroup>");
        sb.AppendLine();
        
        // Collect all unique dependencies
        var allDependencies = conversionResult.ConvertedFiles
            .SelectMany(f => f.Dependencies)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Distinct()
            .OrderBy(d => d)
            .ToList();
        
        if (allDependencies.Any())
        {
            sb.AppendLine("  <ItemGroup>");
            
            // Add common .NET 8 packages
            sb.AppendLine("    <PackageReference Include=\"Microsoft.Extensions.Logging\" Version=\"8.0.0\" />");
            sb.AppendLine("    <PackageReference Include=\"Microsoft.Extensions.DependencyInjection\" Version=\"8.0.0\" />");
            
            // Add dependencies from conversion
            foreach (var dep in allDependencies.Take(10)) // Limit to avoid bloat
            {
                // Try to extract package name and version
                var packageName = ExtractPackageName(dep);
                if (!string.IsNullOrEmpty(packageName))
                {
                    sb.AppendLine($"    <PackageReference Include=\"{packageName}\" Version=\"8.0.0\" />");
                }
            }
            
            sb.AppendLine("  </ItemGroup>");
            sb.AppendLine();
        }
        
        sb.AppendLine("</Project>");
        
        return ($"{projectName}.csproj", sb.ToString());
    }

    /// <summary>
    /// Generates package.json for TypeScript/Angular projects
    /// </summary>
    private (string filename, string content) GenerateRepositoryPackageJson(
        string projectName,
        RepositoryConversionResult conversionResult)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("{");
        sb.AppendLine($"  \"name\": \"{projectName.ToLowerInvariant().Replace(" ", "-")}\",");
        sb.AppendLine("  \"version\": \"1.0.0\",");
        sb.AppendLine("  \"description\": \"Resurrected legacy project\",");
        sb.AppendLine("  \"scripts\": {");
        sb.AppendLine("    \"ng\": \"ng\",");
        sb.AppendLine("    \"start\": \"ng serve\",");
        sb.AppendLine("    \"build\": \"ng build\",");
        sb.AppendLine("    \"test\": \"ng test\",");
        sb.AppendLine("    \"lint\": \"ng lint\"");
        sb.AppendLine("  },");
        sb.AppendLine("  \"dependencies\": {");
        sb.AppendLine("    \"@angular/animations\": \"^17.0.0\",");
        sb.AppendLine("    \"@angular/common\": \"^17.0.0\",");
        sb.AppendLine("    \"@angular/compiler\": \"^17.0.0\",");
        sb.AppendLine("    \"@angular/core\": \"^17.0.0\",");
        sb.AppendLine("    \"@angular/forms\": \"^17.0.0\",");
        sb.AppendLine("    \"@angular/material\": \"^17.0.0\",");
        sb.AppendLine("    \"@angular/platform-browser\": \"^17.0.0\",");
        sb.AppendLine("    \"@angular/platform-browser-dynamic\": \"^17.0.0\",");
        sb.AppendLine("    \"@angular/router\": \"^17.0.0\",");
        sb.AppendLine("    \"rxjs\": \"^7.8.0\",");
        sb.AppendLine("    \"tslib\": \"^2.6.0\",");
        sb.AppendLine("    \"zone.js\": \"^0.14.0\"");
        sb.AppendLine("  },");
        sb.AppendLine("  \"devDependencies\": {");
        sb.AppendLine("    \"@angular-devkit/build-angular\": \"^17.0.0\",");
        sb.AppendLine("    \"@angular/cli\": \"^17.0.0\",");
        sb.AppendLine("    \"@angular/compiler-cli\": \"^17.0.0\",");
        sb.AppendLine("    \"typescript\": \"~5.2.0\"");
        sb.AppendLine("  }");
        sb.AppendLine("}");
        
        return ("package.json", sb.ToString());
    }

    /// <summary>
    /// Gets display name for technology
    /// </summary>
    private string GetTechnologyDisplayName(TechnologyType tech)
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
    /// Gets target technology display name
    /// </summary>
    private string GetTargetTechnologyDisplayName(TechnologyType sourceTech)
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
    /// Gets setup instructions based on technology
    /// </summary>
    private string GetSetupInstructions(TechnologyType tech)
    {
        return tech switch
        {
            TechnologyType.VB6 or TechnologyType.DotNetFramework => @"### Prerequisites
- .NET 8 SDK or later
- Visual Studio 2022 or VS Code

### Setup
```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the project
dotnet run
```",

            TechnologyType.ActionScript or TechnologyType.Silverlight => @"### Prerequisites
- Node.js 18+ and npm
- Angular CLI 17+

### Setup
```bash
# Install dependencies
npm install

# Start development server
npm start

# Build for production
npm run build
```",

            _ => @"### Setup
Please refer to the documentation for your target framework."
        };
    }

    /// <summary>
    /// Extracts package name from dependency string
    /// </summary>
    private string ExtractPackageName(string dependency)
    {
        // Handle various dependency formats
        // "Microsoft.EntityFrameworkCore" or "Microsoft.EntityFrameworkCore (8.0.0)"
        var parts = dependency.Split(new[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : dependency;
    }
}

public interface IPackagingService
{
    Task<byte[]> GenerateZipAsync(ConversionResult result);
    Task<byte[]> PackageRepositoryAsync(
        RepositoryConversionResult conversionResult,
        RepositoryAnalysisResult analysisResult,
        string projectName,
        string? migrationGuide = null,
        CancellationToken cancellationToken = default);
}
