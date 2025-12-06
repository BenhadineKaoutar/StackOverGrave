using StackOverGrave.Api.Models;

namespace StackOverGrave.Api.Services;

public interface IRepositoryConversionService
{
    Task<RepositoryConversionResult> ConvertRepositoryAsync(
        RepositoryAnalysisResult analysis,
        string extractPath,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
