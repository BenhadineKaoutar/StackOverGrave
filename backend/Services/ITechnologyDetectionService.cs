using StackOverGrave.Api.Models;

namespace StackOverGrave.Api.Services;

public interface ITechnologyDetectionService
{
    Task<DeathCertificate> AnalyzeFileAsync(string filePath);
    TechnologyType DetectTechnology(string filename, string content);
}
