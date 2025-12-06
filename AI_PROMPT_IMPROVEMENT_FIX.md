# AI Prompt Improvement - Fix Empty Code Generation

## Problem
When resurrecting files, the AI was returning placeholder comments like `"// .NET 8 code here"` instead of actual converted code, resulting in empty or useless conversion results.

## Root Cause
The prompts contained example JSON responses with placeholder text like:
```json
{
  "convertedCode": "// .NET 8 code here",
  ...
}
```

The AI model was literally copying these placeholders instead of understanding they were examples and generating actual code.

## Solution
1. Added explicit instructions to generate ACTUAL code, not placeholders
2. Emphasized the requirement with "CRITICAL" warnings
3. Wrapped code in markdown code blocks for better context
4. Changed example format to use descriptive placeholders in angle brackets
5. Increased max_tokens from 2000 to 4000 to allow longer responses
6. Enhanced system message to reinforce the requirement

## Changes Made

### AI Conversion Service (`backend/Services/AiConversionService.cs`)

#### 1. Enhanced All Prompts

**Before:**
```csharp
return $@"You are an expert .NET Framework to .NET 8 migration engineer.
...
Respond in JSON format:
{{
  ""convertedCode"": ""// .NET 8 code here"",
  ...
}}";
```

**After:**
```csharp
return $@"You are an expert .NET Framework to .NET 8 migration engineer.

CRITICAL: You MUST provide the COMPLETE converted .NET 8 C# code, not a placeholder or comment. Generate the actual working code.

Requirements:
- Replace System.Web.UI.Page with ASP.NET Core Razor Pages or MVC Controllers
- Replace System.Web.UI controls with Razor syntax or HTML helpers
- Convert code-behind to controller actions or page models
- Replace ViewState with proper state management
...

.NET Framework Code:
```csharp
{code}
```

Respond ONLY with valid JSON in this exact format (no markdown, no code blocks):
{{
  ""convertedCode"": ""<ACTUAL COMPLETE .NET 8 C# CODE HERE - NOT A PLACEHOLDER>"",
  ""migrationNotes"": [""List of migration notes explaining the conversion""],
  ...
}}";
```

**Key improvements:**
- Added "CRITICAL" instruction at the top
- Wrapped source code in markdown code blocks for better context
- Changed placeholder from `"// code here"` to `"<ACTUAL COMPLETE CODE HERE - NOT A PLACEHOLDER>"`
- Added more specific requirements for ASP.NET Web Forms conversion
- Emphasized "ONLY" and "exact format" to prevent markdown wrapping

#### 2. Increased Token Limit

**Before:**
```csharp
var request = new
{
    model = "gpt-4o-mini",
    messages = new[] { ... },
    temperature = 0.2,
    max_tokens = 2000  // Too small for complex code
};
```

**After:**
```csharp
var request = new
{
    model = "gpt-4o-mini",
    messages = new[]
    {
        new { role = "system", content = "You are a code migration expert. You MUST generate complete, working code - never use placeholders or comments like '// code here'. Always respond with valid JSON containing the actual converted code." },
        new { role = "user", content = prompt }
    },
    temperature = 0.3,
    max_tokens = 4000  // Doubled to allow for longer code responses
};
```

**Changes:**
- Increased `max_tokens` from 2000 to 4000 (allows ~3000 words of code)
- Enhanced system message to reinforce no-placeholder requirement
- Slightly increased temperature from 0.2 to 0.3 for more creative solutions

## Prompt Improvements by Technology

### VB6 → C# .NET 8
- Emphasizes preserving business logic
- Requires async/await for I/O
- Mandates dependency injection
- Specifies modern collections (List<T>, Dictionary<TKey,TValue>)
- Requires LINQ usage
- Enforces C# naming conventions

### ActionScript → TypeScript/Angular
- Maps Flash display objects to HTML5 Canvas/DOM
- Converts MovieClip/Sprite to Angular components
- Replaces addEventListener with Angular event binding
- Converts animations to CSS/Angular animations
- Replaces flash.net with HttpClient
- Requires RxJS for async operations

### Silverlight → Angular
- Maps XAML controls to Angular Material
- Converts data binding syntax
- Replaces ICommand with event handlers
- Converts styles to CSS/SCSS
- Uses Angular reactive forms

### .NET Framework → .NET 8 (ASP.NET Web Forms)
- Replaces System.Web.UI.Page with Razor Pages/MVC
- Converts code-behind to controllers/page models
- Replaces ViewState with proper state management
- Replaces Server.MapPath with IWebHostEnvironment
- Replaces HttpContext.Current with DI
- Adds nullable reference types
- Uses modern C# features

## Expected Results

### Before Fix
```json
{
  "convertedCode": "// .NET 8 code here",
  "migrationNotes": ["note1"],
  ...
}
```

### After Fix
```json
{
  "convertedCode": "using Microsoft.AspNetCore.Mvc;\n\npublic class OrderServiceController : Controller\n{\n    private readonly ILogger<OrderServiceController> _logger;\n\n    public OrderServiceController(ILogger<OrderServiceController> logger)\n    {\n        _logger = logger;\n    }\n\n    [HttpGet]\n    public IActionResult Index()\n    {\n        // Actual converted code here\n        return View();\n    }\n}",
  "migrationNotes": [
    "Converted ASP.NET Web Forms Page to MVC Controller",
    "Replaced ViewState with TempData for state management",
    "Added dependency injection for logger"
  ],
  "dependencies": [
    "Microsoft.AspNetCore.Mvc",
    "Microsoft.Extensions.Logging"
  ],
  "breakingChanges": [
    "ViewState is no longer available - use TempData or client-side state",
    "Page lifecycle events replaced with controller actions"
  ],
  "warnings": [
    "Manual testing required for state management",
    "Review authentication/authorization implementation"
  ]
}
```

## Testing

1. **Upload ASP.NET Web Forms file** (e.g., `FrmOrdreService.aspx.vb`)
2. **Analyze** → Should detect as `.NET Framework`
3. **Resurrect** → Should generate actual C# code, not placeholders
4. **View result** → Should see complete, working .NET 8 code

## Token Usage

With the increased limit:
- **Input**: ~500-1500 tokens (prompt + source code)
- **Output**: Up to 4000 tokens (~3000 words of code)
- **Total**: ~5500 tokens per conversion
- **Cost**: ~$0.001 per conversion (gpt-4o-mini pricing)

## Notes

- The `gpt-4o-mini` model is used for cost efficiency (~$0.15 per 1M input tokens, $0.60 per 1M output tokens)
- Temperature set to 0.3 for consistent but slightly creative conversions
- All prompts now use markdown code blocks to provide better context to the AI
- The system message reinforces the requirement across all conversions
