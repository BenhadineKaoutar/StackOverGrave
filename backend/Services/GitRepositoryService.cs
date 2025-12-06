using System.Text.RegularExpressions;
using StackOverGrave.Api.Exceptions;

namespace StackOverGrave.Api.Services;

public class GitRepositoryService : IGitRepositoryService
{
    private readonly ILogger<GitRepositoryService> _logger;
    private readonly HttpClient _httpClient;
    private static readonly string[] AllowedDomains = { "github.com", "gitlab.com", "bitbucket.org" };
    private static readonly string[] BranchFallbacks = { "main", "master", "develop" };
    private const int DownloadTimeoutSeconds = 60;

    public GitRepositoryService(
        ILogger<GitRepositoryService> logger,
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(DownloadTimeoutSeconds);
    }

    public async Task<byte[]> DownloadRepositoryAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📥 Starting repository download from: {Url}", url);

            // Validate and parse URL
            if (!ValidateGitUrl(url, out var parsedUrl) || parsedUrl == null)
            {
                throw new GitRepositoryException(
                    "Invalid or unsupported Git URL",
                    new { url, supportedPlatforms = new[] { "GitHub", "GitLab", "Bitbucket" } },
                    "Please provide a valid GitHub, GitLab, or Bitbucket repository URL"
                );
            }

            // Try downloading with branch fallbacks
            foreach (var branch in BranchFallbacks)
            {
                var zipUrl = ConvertToZipUrl(parsedUrl, branch);
                _logger.LogInformation("🔄 Attempting download with branch: {Branch}", branch);
                _logger.LogInformation("📡 ZIP URL: {ZipUrl}", zipUrl);

                try
                {
                    var response = await _httpClient.GetAsync(zipUrl, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                        _logger.LogInformation("✅ Successfully downloaded repository ({Size} bytes) using branch: {Branch}", 
                            bytes.Length, branch);
                        return bytes;
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("⚠️  Branch '{Branch}' not found, trying next fallback", branch);
                        continue;
                    }

                    // Other error codes should throw
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("⚠️  Branch '{Branch}' not found (404), trying next fallback", branch);
                    continue;
                }
                catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (TaskCanceledException)
                {
                    throw new GitRepositoryException(
                        "Repository download timed out",
                        new { timeoutSeconds = DownloadTimeoutSeconds },
                        "The repository may be too large or the connection is slow. Try again or use a smaller repository"
                    );
                }
            }

            // If we get here, all branches failed
            throw new GitRepositoryException(
                "Repository not found or is private",
                new { triedBranches = BranchFallbacks },
                "Ensure the repository is public and the URL is correct"
            );
        }
        catch (Exception ex) when (ex is not GitRepositoryException)
        {
            _logger.LogError(ex, "❌ Failed to download repository from: {Url}", url);
            throw new GitRepositoryException(
                "Failed to download repository",
                new { error = ex.Message },
                "Please check the URL and try again"
            );
        }
    }

    private bool ValidateGitUrl(string url, out Uri? parsedUrl)
    {
        parsedUrl = null;

        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        // Try to parse as URI
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        // Check if domain is whitelisted
        if (!AllowedDomains.Any(domain => uri.Host.Equals(domain, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("⚠️  Domain not whitelisted: {Host}", uri.Host);
            return false;
        }

        // Prevent SSRF attacks - ensure it's not localhost or private IP
        if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            uri.Host.StartsWith("127.") ||
            uri.Host.StartsWith("192.168.") ||
            uri.Host.StartsWith("10.") ||
            uri.Host.StartsWith("172."))
        {
            _logger.LogWarning("⚠️  Localhost or private IP not allowed: {Host}", uri.Host);
            return false;
        }

        parsedUrl = uri;
        return true;
    }

    private string ConvertToZipUrl(Uri repoUrl, string branch)
    {
        var host = repoUrl.Host.ToLowerInvariant();
        var path = repoUrl.AbsolutePath.TrimEnd('/');

        // Remove .git suffix if present
        if (path.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            path = path.Substring(0, path.Length - 4);
        }

        return host switch
        {
            "github.com" => $"https://github.com{path}/archive/refs/heads/{branch}.zip",
            "gitlab.com" => $"https://gitlab.com{path}/-/archive/{branch}/{GetRepoName(path)}-{branch}.zip",
            "bitbucket.org" => $"https://bitbucket.org{path}/get/{branch}.zip",
            _ => throw new GitRepositoryException(
                "Unsupported Git hosting platform",
                new { host, supportedPlatforms = new[] { "GitHub", "GitLab", "Bitbucket" } },
                "Only GitHub, GitLab, and Bitbucket repositories are supported"
            )
        };
    }

    private string GetRepoName(string path)
    {
        // Extract repository name from path (e.g., /user/repo -> repo)
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length > 0 ? segments[^1] : "repo";
    }
}

public interface IGitRepositoryService
{
    Task<byte[]> DownloadRepositoryAsync(string url, CancellationToken cancellationToken = default);
}
