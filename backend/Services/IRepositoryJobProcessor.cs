using StackOverGrave.Api.Models;

namespace StackOverGrave.Api.Services;

/// <summary>
/// Interface for background repository job processing
/// </summary>
public interface IRepositoryJobProcessor
{
    /// <summary>
    /// Starts a new repository conversion job from Git URL
    /// </summary>
    Task<Guid> StartGitImportJobAsync(string gitUrl);

    /// <summary>
    /// Starts a new repository conversion job from uploaded ZIP
    /// </summary>
    Task<Guid> StartZipUploadJobAsync(string zipFilePath, Guid? jobId = null);

    /// <summary>
    /// Gets the current status of a job
    /// </summary>
    RepositoryJob? GetJobStatus(Guid jobId);

    /// <summary>
    /// Gets the package bytes for a completed job
    /// </summary>
    Task<byte[]?> GetJobPackageAsync(Guid jobId);
}
