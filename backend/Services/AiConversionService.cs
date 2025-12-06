using StackOverGrave.Api.Models;
using StackOverGrave.Api.Exceptions;
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
                throw new AiServiceException(
                    "AI service is not configured",
                    new { missingConfiguration = "OpenAI API key" },
                    "Please contact the administrator to configure the OpenAI API key"
                );
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
            _ => throw new UnsupportedTechnologyException(
                "Unsupported technology type",
                new { technology = sourceTech.ToString() },
                "Currently supported: VB6, Flash/ActionScript, Silverlight, .NET Framework"
            )
        };

        return basePrompt;
    }

    private string BuildVB6Prompt(string code)
    {
        return $@"You are an expert software migration engineer. Convert the following VB6 code to modern C# .NET 8.

CRITICAL: You MUST provide the COMPLETE converted C# code, not a placeholder or comment. Generate the actual working code.

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
```vb
{code}
```

Respond ONLY with valid JSON in this exact format (no markdown, no code blocks):
{{
  ""convertedCode"": ""<ACTUAL COMPLETE C# CODE HERE - NOT A PLACEHOLDER>"",
  ""migrationNotes"": [""List of migration notes""],
  ""dependencies"": [""List of required NuGet packages""],
  ""breakingChanges"": [""List of breaking changes""],
  ""warnings"": [""List of warnings""]
}}";
    }

    private string BuildActionScriptPrompt(string code)
    {
        return $@"You are an expert Flash/ActionScript to modern web migration engineer. Convert the following ActionScript code to TypeScript for use in Angular 17+.

CRITICAL: You MUST provide the COMPLETE converted TypeScript code, not a placeholder or comment. Generate the actual working code.

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
```actionscript
{code}
```

Respond ONLY with valid JSON in this exact format (no markdown, no code blocks):
{{
  ""convertedCode"": ""<ACTUAL COMPLETE TYPESCRIPT CODE HERE - NOT A PLACEHOLDER>"",
  ""migrationNotes"": [""List of migration notes""],
  ""dependencies"": [""@angular/core"", ""rxjs""],
  ""breakingChanges"": [""List of breaking changes""],
  ""warnings"": [""List of warnings""]
}}";
    }

    private string BuildSilverlightPrompt(string code)
    {
        return $@"You are an expert Silverlight to Angular migration engineer. Convert the following Silverlight XAML to Angular component.

CRITICAL: You MUST provide the COMPLETE converted Angular TypeScript code, not a placeholder or comment. Generate the actual working code.

Requirements:
- Map XAML controls to Angular Material components
- Convert data binding syntax to Angular binding
- Replace ICommand with Angular event handlers
- Convert styles to CSS/SCSS
- Use Angular reactive forms for form validation

Silverlight XAML:
```xaml
{code}
```

Respond ONLY with valid JSON in this exact format (no markdown, no code blocks):
{{
  ""convertedCode"": ""<ACTUAL COMPLETE ANGULAR TYPESCRIPT CODE HERE - NOT A PLACEHOLDER>"",
  ""migrationNotes"": [""List of migration notes""],
  ""dependencies"": [""@angular/material""],
  ""breakingChanges"": [""List of breaking changes""],
  ""warnings"": [""List of warnings""]
}}";
    }

    private string BuildDotNetFrameworkPrompt(string code)
    {
        return $@"You are an expert .NET Framework to .NET 8 migration engineer. Modernize the following .NET Framework code to .NET 8.

CRITICAL: You MUST provide the COMPLETE converted .NET 8 C# code, not a placeholder or comment. Generate the actual working code.

Requirements:
- Replace System.Web.UI.Page with ASP.NET Core Razor Pages or MVC Controllers
- Replace System.Web.UI controls with Razor syntax or HTML helpers
- Convert code-behind to controller actions or page models
- Replace ViewState with proper state management (TempData, Session, or client-side)
- Replace Server.MapPath with IWebHostEnvironment
- Replace HttpContext.Current with dependency-injected HttpContext
- Add nullable reference types
- Use modern C# features (pattern matching, records, init properties)
- Use built-in dependency injection
- Update to latest NuGet package versions

.NET Framework Code:
```csharp
{code}
```

Respond ONLY with valid JSON in this exact format (no markdown, no code blocks):
{{
  ""convertedCode"": ""<ACTUAL COMPLETE .NET 8 C# CODE HERE - NOT A PLACEHOLDER>"",
  ""migrationNotes"": [""List of migration notes explaining the conversion""],
  ""dependencies"": [""List of required NuGet packages""],
  ""breakingChanges"": [""List of breaking changes from original code""],
  ""warnings"": [""List of warnings or manual review items""]
}}";
    }

    private async Task<string> CallOpenAiAsync(string prompt, CancellationToken cancellationToken)
    {
        var request = new
        {
            model = "gpt-4o-mini", // Using gpt-4o-mini - faster and cheaper
            messages = new[]
            {
                new { role = "system", content = "You are a code migration expert. You MUST generate complete, working code - never use placeholders or comments like '// code here'. Always respond with valid JSON containing the actual converted code." },
                new { role = "user", content = prompt }
            },
            temperature = 0.3,
            max_tokens = 4000  // Increased to allow for longer code responses
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
            throw new AiServiceException(
                "AI service returned an invalid response",
                new { error = "No choices in response" },
                "Please try again in a few moments"
            );
        }

        var messageContent = responseObj.Choices[0]?.Message?.Content;
        if (string.IsNullOrEmpty(messageContent))
        {
            _logger.LogError("❌ Empty content in OpenAI response");
            throw new AiServiceException(
                "AI service returned an empty response",
                new { error = "Empty content" },
                "Please try again in a few moments"
            );
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

                return result ?? throw new AiServiceException(
                    "Failed to parse AI conversion result",
                    new { error = "Invalid JSON structure" },
                    "Please try again"
                );
            }

            throw new AiServiceException(
                "AI service response format is invalid",
                new { error = "No JSON found in response" },
                "Please try again"
            );
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
