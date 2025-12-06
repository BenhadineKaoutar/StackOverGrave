using StackOverGrave.Api.Models;

namespace StackOverGrave.Api.Services;

public interface IMigrationGuideService
{
    Task<string> GenerateMigrationGuideAsync(
        RepositoryConversionResult conversionResult,
        RepositoryAnalysisResult analysisResult,
        CancellationToken cancellationToken = default);
}
