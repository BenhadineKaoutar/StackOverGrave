using StackOverGrave.Api.Models;
using System.Text.Json;

namespace StackOverGrave.Api.Services;

public class AiConversionService : IAiConversionService
{
    private readonly ILogger<AiConversionService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _openAiApiKey;

    public AiConversionService(
        ILogger<AiConversionService> logger,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _openAiApiKey = configuration["OpenAI:ApiKey"] ?? "";
        
        if (string.IsNullOrWhiteSpace(_openAiApiKey))
        {
            _logger.LogError("❌ OpenAI API key is not configured!");
        }
    }

    public async Task<ConversionResultDto> ConvertCodeAsync(
        string code,
        TechnologyType sourceTech,
        TechnologyType targetTech,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_openAiApiKey))
            {
                throw new InvalidOperationException("OpenAI API key is not configured. Please add it to appsettings.json");
            }

            _logger.LogInformation("🤖 Building prompt for {Source} conversion", sourceTech);
            var prompt = BuildPrompt(code, sourceTech);
            
            _logger.LogInformation("📡 Calling OpenAI API (model: gpt-4o-mini)");
            var response = await CallOpenAiAsync(prompt, cancellationToken);
            
            _logger.LogInformation("📝 Parsing OpenAI response");
            var result = ParseResponse(response);

            _logger.LogInformation("✅ Conversion completed: {Source} -> {Target}", sourceTech, targetTech);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ AI conversion failed for {Source}", sourceTech);
            throw;
        }
    }

    private string BuildPrompt(string code, TechnologyType sourceTech)
    {
        var basePrompt = sourceTech switch
        {
            TechnologyType.VB6 => BuildVB6Prompt(code),
            TechnologyType.ActionScript => BuildActionScriptPrompt(code),
            TechnologyType.Silverlight => BuildSilverlightPrompt(code),
            TechnologyType.DotNetFramework => BuildDotNetFrameworkPrompt(code),
            _ => throw new ArgumentException("Unsupported technology type")
        };

        return basePrompt;
    }

    private string BuildVB6Prompt(string code)
    {
        return $@"You are an expert software migration engineer. Convert the following VB6 code to modern C# .NET 8.

Requirements:
- Preserve all business logic exactly
- Use async/await for I/O operations
- Implement dependency injection where appropriate
- Replace VB6 collections with modern C# collections (List<T>, Dictionary<TKey,TValue>)
- Use LINQ instead of loops where appropriate
- Add nullable reference types
- Follow C# naming conventions (PascalCase for methods, camelCase for variables)
- Replace MsgBox with proper logging or exceptions

VB6 Code:
{code}

Respond in JSON format:
{{
  ""convertedCode"": ""// C# code here"",
  ""migrationNotes"": [""note1"", ""note2""],
  ""dependencies"": [""package1"", ""package2""],
  ""breakingChanges"": [""change1"", ""change2""],
  ""warnings"": [""warning1"", ""warning2""]
}}";
    }

    private string BuildActionScriptPrompt(string code)
    {
        return $@"You are an expert Flash/ActionScript to modern web migration engineer. Convert the following ActionScript code to TypeScript for use in Angular 17+.

Requirements:
- Replace Flash display objects with HTML5 Canvas or DOM elements
- Convert MovieClip/Sprite to Angular components
- Replace addEventListener with Angular event binding
- Convert timeline animations to CSS animations or Angular animations
- Replace flash.net with HttpClient
- Use RxJS for async operations
- Follow Angular style guide
- Add TypeScript strict types

ActionScript Code:
{code}

Respond in JSON format:
{{
  ""convertedCode"": ""// TypeScript code here"",
  ""migrationNotes"": [""note1"", ""note2""],
  ""dependencies"": [""@angular/core"", ""rxjs""],
  ""breakingChanges"": [""change1"", ""change2""],
  ""warnings"": [""warning1"", ""warning2""]
}}";
    }

    private string BuildSilverlightPrompt(string code)
    {
        return $@"You are an expert Silverlight to Angular migration engineer. Convert the following Silverlight XAML to Angular component.

Requirements:
- Map XAML controls to Angular Material components
- Convert data binding syntax to Angular binding
- Replace ICommand with Angular event handlers
- Convert styles to CSS/SCSS
- Use Angular reactive forms for form validation

Silverlight XAML:
{code}

Respond in JSON format:
{{
  ""convertedCode"": ""// Angular component code here"",
  ""migrationNotes"": [""note1"", ""note2""],
  ""dependencies"": [""@angular/material""],
  ""breakingChanges"": [""change1"", ""change2""],
  ""warnings"": [""warning1"", ""warning2""]
}}";
    }

    private string BuildDotNetFrameworkPrompt(string code)
    {
        return $@"You are an expert .NET Framework to .NET 8 migration engineer. Modernize the following .NET Framework code to .NET 8.

Requirements:
- Replace deprecated APIs with modern equivalents
- Add nullable reference types
- Use modern C# features (pattern matching, records, init properties)
- Replace System.Web with ASP.NET Core equivalents
- Use built-in dependency injection
- Update to latest NuGet package versions

.NET Framework Code:
{code}

Respond in JSON format:
{{
  ""convertedCode"": ""// .NET 8 code here"",
  ""migrationNotes"": [""note1"", ""note2""],
  ""dependencies"": [""package1"", ""package2""],
  ""breakingChanges"": [""change1"", ""change2""],
  ""warnings"": [""warning1"", ""warning2""]
}}";
    }

    private async Task<string> CallOpenAiAsync(string prompt, CancellationToken cancellationToken)
    {
        var request = new
        {
            model = "gpt-4o-mini", // Using gpt-4o-mini - faster and cheaper
            messages = new[]
            {
                new { role = "system", content = "You are a code migration expert. Always respond with valid JSON." },
                new { role = "user", content = prompt }
            },
            temperature = 0.2,
            max_tokens = 2000
        };

        var requestJson = JsonSerializer.Serialize(request);
        var httpContent = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

        _logger.LogInformation("📡 Sending request to OpenAI API...");
        
        var response = await _httpClient.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            httpContent,
            cancellationToken
        );

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("❌ OpenAI API error: {Status} - {Response}", response.StatusCode, responseJson);
            response.EnsureSuccessStatusCode();
        }
        
        _logger.LogInformation("✅ Received response from OpenAI API");
        _logger.LogInformation("📝 Response preview: {Preview}", responseJson.Length > 200 ? responseJson.Substring(0, 200) + "..." : responseJson);
        
        var responseObj = JsonSerializer.Deserialize<OpenAiResponse>(responseJson, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        if (responseObj?.Choices == null || responseObj.Choices.Count == 0)
        {
            _logger.LogError("❌ No choices in OpenAI response: {Response}", responseJson);
            throw new Exception("Invalid OpenAI response - no choices");
        }

        var messageContent = responseObj.Choices[0]?.Message?.Content;
        if (string.IsNullOrEmpty(messageContent))
        {
            _logger.LogError("❌ Empty content in OpenAI response");
            throw new Exception("Invalid OpenAI response - empty content");
        }

        return messageContent;
    }

    private ConversionResultDto ParseResponse(string response)
    {
        try
        {
            // Try to extract JSON from markdown code blocks if present
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var result = JsonSerializer.Deserialize<ConversionResultDto>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? throw new Exception("Failed to parse conversion result");
            }

            throw new Exception("No JSON found in response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse AI response");
            
            // Return a fallback result
            return new ConversionResultDto
            {
                ConvertedCode = response,
                MigrationNotes = new List<string> { "AI response could not be parsed. Manual review required." },
                Dependencies = new List<string>(),
                BreakingChanges = new List<string> { "Unable to determine breaking changes automatically" },
                Warnings = new List<string> { "Response parsing failed. Please review the converted code carefully." }
            };
        }
    }

    private class OpenAiResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    private class Message
    {
        [System.Text.Json.Serialization.JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}

public interface IAiConversionService
{
    Task<ConversionResultDto> ConvertCodeAsync(
        string code,
        TechnologyType sourceTech,
        TechnologyType targetTech,
        CancellationToken cancellationToken = default);
}

public class ConversionResultDto
{
    public string ConvertedCode { get; set; } = string.Empty;
    public List<string> MigrationNotes { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    public List<string> BreakingChanges { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
