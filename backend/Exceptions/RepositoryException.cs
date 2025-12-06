namespace StackOverGrave.Api.Exceptions;

/// <summary>
/// Base exception for repository-related errors with user-friendly messages
/// </summary>
public class RepositoryException : Exception
{
    public string UserMessage { get; set; }
    public object? Details { get; set; }
    public string? Suggestion { get; set; }
    public int StatusCode { get; set; } = 400;
    
    public RepositoryException(string userMessage, object? details = null, string? suggestion = null, int statusCode = 400)
        : base(userMessage)
    {
        UserMessage = userMessage;
        Details = details;
        Suggestion = suggestion;
        StatusCode = statusCode;
    }
}

/// <summary>
/// Exception for Git repository access errors (404, private repos, etc.)
/// </summary>
public class GitRepositoryException : RepositoryException
{
    public GitRepositoryException(string userMessage, object? details = null, string? suggestion = null)
        : base(userMessage, details, suggestion ?? "Ensure the repository is public and the URL is correct", 404)
    {
    }
}

/// <summary>
/// Exception for file size limit violations
/// </summary>
public class FileSizeLimitException : RepositoryException
{
    public FileSizeLimitException(string userMessage, object? details = null, string? suggestion = null)
        : base(userMessage, details, suggestion ?? "Try uploading a smaller file or reducing the repository size", 400)
    {
    }
}

/// <summary>
/// Exception for repository size limit violations (file count, LOC, etc.)
/// </summary>
public class RepositorySizeLimitException : RepositoryException
{
    public RepositorySizeLimitException(string userMessage, object? details = null, string? suggestion = null)
        : base(userMessage, details, suggestion ?? "Consider splitting the repository or selecting specific directories", 400)
    {
    }
}

/// <summary>
/// Exception for invalid file format or structure
/// </summary>
public class InvalidFileFormatException : RepositoryException
{
    public InvalidFileFormatException(string userMessage, object? details = null, string? suggestion = null)
        : base(userMessage, details, suggestion ?? "Ensure the file is a valid ZIP archive", 400)
    {
    }
}

/// <summary>
/// Exception for AI service errors (OpenAI API issues)
/// </summary>
public class AiServiceException : RepositoryException
{
    public AiServiceException(string userMessage, object? details = null, string? suggestion = null)
        : base(userMessage, details, suggestion ?? "Please try again in a few moments", 503)
    {
    }
}

/// <summary>
/// Exception for unsupported technology types
/// </summary>
public class UnsupportedTechnologyException : RepositoryException
{
    public UnsupportedTechnologyException(string userMessage, object? details = null, string? suggestion = null)
        : base(userMessage, details, suggestion ?? "Currently supported: VB6, Flash/ActionScript, Silverlight, .NET Framework", 400)
    {
    }
}
