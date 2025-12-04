# File Packaging Spec

## Overview
Generate downloadable .zip files containing converted code with proper project structure and documentation.

## Package Contents

### For .NET 8 Projects
```
converted-project.zip
├── src/
│   ├── Program.cs
│   ├── ConvertedCode.cs
│   └── appsettings.json
├── ConvertedProject.csproj
├── README.md
└── MIGRATION_NOTES.md
```

### For TypeScript/Angular Projects
```
converted-project.zip
├── src/
│   ├── app/
│   │   ├── converted.component.ts
│   │   ├── converted.component.html
│   │   └── converted.component.scss
│   └── main.ts
├── package.json
├── tsconfig.json
├── angular.json
├── README.md
└── MIGRATION_NOTES.md
```

## README.md Template

```markdown
# Converted Project

## Original Technology
{sourceTech} - Deprecated {deprecatedDate}

## New Technology
{targetTech}

## Quick Start
{installation instructions}

## Dependencies
{list of packages}

## Breaking Changes
{list of breaking changes}

## Manual Steps Required
{list of manual steps}
```

## Backend Implementation

```csharp
public interface IPackagingService
{
    Task<byte[]> GenerateZipAsync(ConversionResult result);
}
```
