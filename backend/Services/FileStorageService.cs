using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using StackOverGrave.Api.Exceptions;

namespace StackOverGrave.Api.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly string _tempPath;
    private readonly ILogger<FileStorageService> _logger;
    private readonly long _maxFileSizeBytes;
    private readonly long _maxExtractionSizeBytes;

    public FileStorageService(ILogger<FileStorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _uploadPath = configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _tempPath = configuration["Repository:TempDirectory"] ?? Path.Combine(Path.GetTempPath(), "stackovergrave");
        
        // Default to 50MB for uploads (requirement 2.1)
        _maxFileSizeBytes = (configuration.GetValue<long?>("Repository:MaxFileSizeMB") ?? 50) * 1024 * 1024;
        
        // Default to 200MB for extraction (zip bomb protection)
        _maxExtractionSizeBytes = (configuration.GetValue<long?>("Repository:MaxExtractionSizeMB") ?? 200) * 1024 * 1024;
        
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
        
        if (!Directory.Exists(_tempPath))
        {
            Directory.CreateDirectory(_tempPath);
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, Guid projectId)
    {
        var fileName = $"{projectId}_{file.FileName}";
        var filePath = Path.Combine(_uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("File saved: {FilePath}", filePath);
        return filePath;
    }

    public async Task<string> SaveZipFileAsync(IFormFile file)
    {
        // Validate file size (requirement 2.1)
        if (file.Length > _maxFileSizeBytes)
        {
            throw new FileSizeLimitException(
                "File size exceeds the maximum allowed limit",
                new { 
                    actualSizeMB = Math.Round(file.Length / (1024.0 * 1024.0), 2),
                    maxSizeMB = _maxFileSizeBytes / (1024 * 1024)
                },
                "Try compressing your repository or removing unnecessary files"
            );
        }

        // Validate ZIP file format (requirement 2.2)
        if (!IsValidZipFile(file))
        {
            throw new InvalidFileFormatException(
                "Invalid file format",
                new { fileName = file.FileName },
                "Please upload a valid ZIP file"
            );
        }

        // Create GUID-based temporary file naming (requirement 2.2)
        var tempFileName = $"{Guid.NewGuid()}.zip";
        var tempFilePath = Path.Combine(_tempPath, tempFileName);

        // Validate file path security (requirement 2.2)
        if (!IsSecureFilePath(tempFilePath))
        {
            throw new RepositoryException(
                "Invalid file path detected",
                null,
                "Please contact support if this error persists"
            );
        }

        using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("ZIP file saved: {FilePath}, Size: {Size} bytes", tempFilePath, file.Length);
        return tempFilePath;
    }

    public async Task<string> SaveZipBytesAsync(byte[] zipBytes)
    {
        // Validate file size
        if (zipBytes.Length > _maxFileSizeBytes)
        {
            throw new FileSizeLimitException(
                "Downloaded repository exceeds the maximum allowed size",
                new { 
                    actualSizeMB = Math.Round(zipBytes.Length / (1024.0 * 1024.0), 2),
                    maxSizeMB = _maxFileSizeBytes / (1024 * 1024)
                },
                "The repository is too large to process"
            );
        }

        // Create GUID-based temporary file naming
        var tempFileName = $"{Guid.NewGuid()}.zip";
        var tempFilePath = Path.Combine(_tempPath, tempFileName);

        await File.WriteAllBytesAsync(tempFilePath, zipBytes);

        _logger.LogInformation("ZIP bytes saved: {FilePath}, Size: {Size} bytes", tempFilePath, zipBytes.Length);
        return tempFilePath;
    }

    public async Task<string> ExtractZipAsync(string zipFilePath)
    {
        if (!File.Exists(zipFilePath))
        {
            throw new InvalidFileFormatException(
                "ZIP file not found",
                new { filePath = zipFilePath },
                "The uploaded file may have been deleted"
            );
        }

        // Create extraction directory with GUID
        var extractionDir = Path.Combine(_tempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(extractionDir);

        try
        {
            long totalExtractedSize = 0;

            using (var archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    // Skip directory entries
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    // Validate extracted file paths (requirement 2.3 - prevent directory traversal)
                    var destinationPath = Path.GetFullPath(Path.Combine(extractionDir, entry.FullName));
                    var normalizedExtractionDir = Path.GetFullPath(extractionDir);
                    
                    // Ensure the path ends with directory separator for proper comparison
                    if (!normalizedExtractionDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        normalizedExtractionDir += Path.DirectorySeparatorChar;
                    }
                    
                    if (!destinationPath.StartsWith(normalizedExtractionDir, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Path validation failed: destination={Destination}, base={Base}", destinationPath, normalizedExtractionDir);
                        throw new InvalidFileFormatException(
                            "Invalid file path detected in ZIP archive",
                            new { fileName = entry.FullName },
                            "The ZIP file contains potentially malicious paths"
                        );
                    }

                    // Zip bomb protection (requirement 2.3 - limit extraction size)
                    totalExtractedSize += entry.Length;
                    if (totalExtractedSize > _maxExtractionSizeBytes)
                    {
                        throw new FileSizeLimitException(
                            "ZIP file extraction size exceeds the maximum allowed limit",
                            new { 
                                extractedSizeMB = Math.Round(totalExtractedSize / (1024.0 * 1024.0), 2),
                                maxSizeMB = _maxExtractionSizeBytes / (1024 * 1024)
                            },
                            "The ZIP file may be a zip bomb or contains too much data"
                        );
                    }

                    // Create directory if needed
                    var entryDirectory = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(entryDirectory) && !Directory.Exists(entryDirectory))
                    {
                        Directory.CreateDirectory(entryDirectory);
                    }

                    // Extract file
                    entry.ExtractToFile(destinationPath, overwrite: true);
                }
            }

            _logger.LogInformation("ZIP extracted: {ZipPath} -> {ExtractPath}, Total size: {Size} bytes", 
                zipFilePath, extractionDir, totalExtractedSize);
            
            return extractionDir;
        }
        catch (InvalidDataException ex)
        {
            // Handle extraction errors gracefully (requirement 2.4)
            _logger.LogError(ex, "Failed to extract ZIP file: {ZipPath}", zipFilePath);
            
            // Cleanup on failure
            if (Directory.Exists(extractionDir))
            {
                Directory.Delete(extractionDir, recursive: true);
            }
            
            throw new InvalidFileFormatException(
                "Failed to extract ZIP file",
                new { error = "The file may be corrupted or not a valid ZIP archive" },
                "Please verify the file is a valid ZIP and try again"
            );
        }
        catch (RepositoryException)
        {
            // Cleanup on failure
            if (Directory.Exists(extractionDir))
            {
                Directory.Delete(extractionDir, recursive: true);
            }
            throw;
        }
        catch (Exception ex)
        {
            // Handle extraction errors gracefully (requirement 2.4)
            _logger.LogError(ex, "Unexpected error extracting ZIP file: {ZipPath}", zipFilePath);
            
            // Cleanup on failure
            if (Directory.Exists(extractionDir))
            {
                Directory.Delete(extractionDir, recursive: true);
            }
            
            throw new RepositoryException(
                "An unexpected error occurred while extracting the ZIP file",
                null,
                "Please try again or contact support if the problem persists"
            );
        }
    }

    public async Task<string> ReadFileAsync(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }

    public Task DeleteFileAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("File deleted: {FilePath}", filePath);
        }
        return Task.CompletedTask;
    }

    public Task DeleteDirectoryAsync(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
            _logger.LogInformation("Directory deleted: {DirectoryPath}", directoryPath);
        }
        return Task.CompletedTask;
    }

    private bool IsValidZipFile(IFormFile file)
    {
        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".zip")
        {
            return false;
        }

        // Check magic number (ZIP file signature: PK)
        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            
            // ZIP files start with PK (0x50 0x4B)
            return buffer[0] == 0x50 && buffer[1] == 0x4B;
        }
        catch
        {
            return false;
        }
    }

    private bool IsSecureFilePath(string filePath)
    {
        // Prevent directory traversal attacks
        var fullPath = Path.GetFullPath(filePath);
        var tempPathFull = Path.GetFullPath(_tempPath);
        
        return fullPath.StartsWith(tempPathFull, StringComparison.OrdinalIgnoreCase);
    }
}

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, Guid projectId);
    Task<string> SaveZipFileAsync(IFormFile file);
    Task<string> SaveZipBytesAsync(byte[] zipBytes);
    Task<string> ExtractZipAsync(string zipFilePath);
    Task<string> ReadFileAsync(string filePath);
    Task DeleteFileAsync(string filePath);
    Task DeleteDirectoryAsync(string directoryPath);
}
