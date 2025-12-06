using StackOverGrave.Api.Models;

namespace StackOverGrave.Api.Services;

public interface IRepositoryAnalysisService
{
    Task<RepositoryAnalysisResult> AnalyzeRepositoryAsync(string extractPath, CancellationToken cancellationToken = default);
}
