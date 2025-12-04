# Legacy Parser Spec

## Overview
Technology detection service that analyzes uploaded files to identify legacy frameworks and generate "death certificates" with metadata.

## Supported Technologies
- VB6 (Visual Basic 6)
- Flash ActionScript 2.0/3.0
- Silverlight
- .NET Framework (< 4.8)

## Detection Rules

### VB6 Detection
- File extension: `.vb`, `.frm`, `.bas`, `.cls`
- Syntax patterns:
  - `Attribute VB_Name`
  - `Option Explicit`
  - `Dim ... As ...` (VB6 style)
  - `Set ... = New`
  - `MsgBox`, `InputBox`
  - `Form_Load`, `Command1_Click` event patterns

### ActionScript Detection
- File extension: `.as`, `.fla`
- Syntax patterns:
  - `package { ... }`
  - `import flash.`
  - `extends Sprite`, `extends MovieClip`
  - `addEventListener`
  - `stage.`, `root.`
  - ActionScript 2: `_root`, `_parent`, `onEnterFrame`

### Silverlight Detection
- File extension: `.xaml`, `.xaml.cs`
- Content patterns:
  - `xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"`
  - `xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"`
  - Silverlight-specific controls: `<UserControl`, `<Canvas`, `<Grid`
  - `Silverlight` in assembly references

### Old .NET Framework Detection
- File extension: `.csproj`, `.vbproj`, `.cs`
- Content patterns:
  - `<TargetFramework>net4[0-7]</TargetFramework>`
  - `<TargetFrameworkVersion>v4.[0-7]</TargetFrameworkVersion>`
  - Legacy namespaces: `System.Web.Mvc`, `System.ServiceModel`
  - `packages.config` references

## Death Certificate Data

### Structure
```typescript
interface DeathCertificate {
  technology: TechnologyType;
  originalFilename: string;
  detectedVersion?: string;
  deprecatedDate: Date;
  causeOfDeath: string;
  fileStats: {
    linesOfCode: number;
    fileSize: number;
    complexity: 'Low' | 'Medium' | 'High';
  };
  warnings: string[];
}
```

### Cause of Death Messages
- VB6: "Abandoned by Microsoft in favor of .NET"
- Flash: "Killed by Steve Jobs and HTML5"
- Silverlight: "Murdered by responsive web design"
- Old .NET Framework: "Replaced by .NET Core/.NET 5+"

### Deprecated Dates (Research)
- VB6: April 8, 2008
- Flash: December 31, 2020
- Silverlight: October 12, 2021
- .NET Framework 4.x: Still supported but legacy

## Implementation

### Backend Service (.NET 8)
```csharp
public interface ITechnologyDetectionService
{
    Task<DeathCertificate> AnalyzeFileAsync(string filePath);
    TechnologyType DetectTechnology(string filename, string content);
    FileStatistics CalculateStatistics(string content);
}
```

### Detection Algorithm
1. Check file extension first
2. Read file content (first 1000 lines for performance)
3. Apply regex patterns for each technology
4. Calculate confidence score (0-100)
5. Return technology with highest confidence
6. If confidence < 50, return "Unknown"

### File Statistics
- Lines of Code: Count non-empty, non-comment lines
- Complexity: 
  - Low: < 100 LOC, < 5 methods
  - Medium: 100-500 LOC, 5-20 methods
  - High: > 500 LOC, > 20 methods

## Error Handling
- Unsupported file type → Return error with supported types
- File too large (> 5MB) → Return size limit error
- Corrupted file → Return parsing error
- Multiple technologies detected → Return primary + warning

## Testing Requirements
- Unit tests for each detection pattern
- Test with real legacy code samples
- Edge cases: empty files, mixed technologies, modern code
- Performance: Should complete in < 1 second

## Future Enhancements
- Support for: ColdFusion, Perl CGI, Classic ASP, Java Applets
- Machine learning model for better detection
- Dependency analysis (detect libraries used)
