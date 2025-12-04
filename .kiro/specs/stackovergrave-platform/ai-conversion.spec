version Spec

## Overview
OpenAI integration service that converts legacy code to modern equivalents using GPT-4 with specialized prompts.

## Conversion Paths

### VB6 → C# .NET 8
**Target:** Modern C# with async/await, dependency injection, LINQ

**Prompt Template:**
```
You are an expert software migration engineer. Convert the following VB6 code to modern C# .NET 8.

Requirements:
- Preserve all business logic exactly
- Use async/await for I/O operations
- Implement dependency injection where appropriate
- Replace VB6 collections with modern C# collections (List<T>, Dictionary<TKey,TValue>)
- Use LINQ instead of loops where appropriate
- Add nullable reference types
- Follow C# naming conventions (PascalCase for methods, camelCase for variables)
- Add XML documentation comments
- Replace MsgBox with proper logging or exceptions

VB6 Code:
{code}

Provide:
1. Converted C# code
2. Migration notes explaining major changes
3. List of NuGet packages needed
4. Breaking changes or manual steps required
```

### ActionScript → TypeScript/Angular
**Target:** TypeScript suitable for Angular 17+ with modern web APIs

**Prompt Template:**
```
You are an expert Flash/ActionScript to modern web migration engineer. Convert the following ActionScript code to TypeScript for use in Angular 17+.

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

Provide:
1. Converted TypeScript code
2. Angular component structure (if applicable)
3. HTML template (if UI elements exist)
4. CSS for animations
5. Migration notes explaining Flash API replacements
6. npm packages needed
```

### Silverlight XAML → Angular
**Target:** Angular components with Angular Material

**Prompt Template:**
```
You are an expert Silverlight to Angular migration engineer. Convert the following Silverlight XAML to Angular component.

Requirements:
- Map XAML controls to Angular Material components:
  - TextBox → mat-input
  - Button → mat-button
  - DataGrid → mat-table
  - ComboBox → mat-select
  - ListBox → mat-list
- Convert data binding syntax (Binding Path=X) to Angular binding [(ngModel)]
- Replace ICommand with Angular event handlers
- Convert styles to CSS/SCSS
- Use Angular reactive forms for form validation

Silverlight XAML:
{code}

Provide:
1. Angular component TypeScript
2. HTML template
3. SCSS styles
4. Migration notes for data binding changes
5. Angular Material modules needed
```

### Old .NET Framework → .NET 8
**Target:** Modern .NET 8 with latest C# features

**Prompt Template:**
```
You are an expert .NET Framework to .NET 8 migration engineer. Modernize the following .NET Framework code to .NET 8.

Requirements:
- Replace deprecated APIs with modern equivalents
- Add nullable reference types
- Use modern C# features (pattern matching, records, init properties)
- Replace System.Web with ASP.NET Core equivalents
- Convert WCF to gRPC or REST API
- Use built-in dependency injection
- Replace packages.config with PackageReference
- Update to latest NuGet package versions

.NET Framework Code:
{code}

Provide:
1. Modernized .NET 8 code
2. Migration notes for API changes
3. Updated NuGet packages
4. Breaking changes requiring manual intervention
5. Configuration changes (web.config → appsettings.json)
```

## Response Parsing

### Expected AI Response Format
```json
{
  "convertedCode": "// Modern code here",
  "migrationNotes": [
    "Replaced VB6 collections with List<T>",
    "Converted synchronous file I/O to async"
  ],
  "dependencies": [
    "Microsoft.Extensions.DependencyInjection",
    "Serilog"
  ],
  "breakingChanges": [
    "Manual database connection string update required"
  ],
  "warnings": [
    "Complex COM interop may need manual review"
  ]
}
```

## Implementation

### Backend Service (.NET 8)
```csharp
public interface IAiConversionService
{
    Task<ConversionResult> ConvertCodeAsync(
        string code, 
        TechnologyType sourceTech, 
        TechnologyType targetTech,
        CancellationToken cancellationToken = default);
}

public class ConversionResult
{
    public string ConvertedCode { get; set; }
    public List<string> MigrationNotes { get; set; }
    public List<string> Dependencies { get; set; }
    public List<string> BreakingChanges { get; set; }
    public List<string> Warnings { get; set; }
    public int TokensUsed { get; set; }
    public decimal EstimatedCost { get; set; }
}
```

## OpenAI Configuration

### Model Selection
- Primary: `gpt-4-turbo-preview` (best quality)
- Fallback: `gpt-3.5-turbo` (faster, cheaper)

### Token Limits
- Max input tokens: 4000
- Max output tokens: 2000
- Temperature: 0.2 (more deterministic)
- Top P: 0.9

### Rate Limiting
- Max 10 requests per minute
- Queue additional requests
- Show progress to user

### Cost Tracking
- Log tokens used per conversion
- Calculate cost (GPT-4: $0.03/1K input, $0.06/1K output)
- Display to user in admin panel

## Error Handling

### API Errors
- Rate limit exceeded → Queue and retry after delay
- Invalid API key → Show configuration error
- Timeout (> 30s) → Cancel and show error
- Token limit exceeded → Split code into chunks

### Code Quality Issues
- AI returns invalid syntax → Show warning, allow manual edit
- Incomplete conversion → Highlight missing sections
- Hallucinated code → Add disclaimer about manual review

## Caching Strategy

### Cache Key
`{sourceTech}:{targetTech}:{codeHash}`

### Cache Storage
- Use in-memory cache (IMemoryCache)
- TTL: 24 hours
- Max size: 100 conversions

### Cache Invalidation
- Clear on API key change
- Clear on prompt template update

## Testing Requirements
- Unit tests with mocked OpenAI responses
- Integration tests with real API (limited)
- Test each conversion path with sample code
- Validate response parsing
- Test error scenarios

## MCP Integration

### Configuration
```json
{
  "mcpServers": {
    "openai": {
      "command": "uvx",
      "args": ["mcp-server-openai"],
      "env": {
        "OPENAI_API_KEY": "${OPENAI_API_KEY}"
      }
    }
  }
}
```

### Usage
- Use MCP tools for OpenAI API calls
- Leverage MCP's built-in retry logic
- Monitor MCP logs for debugging

## Future Enhancements
- Fine-tuned model for code conversion
- Multi-file project conversion
- Incremental conversion (convert one class at a time)
- A/B testing different prompts
- User feedback loop to improve prompts
