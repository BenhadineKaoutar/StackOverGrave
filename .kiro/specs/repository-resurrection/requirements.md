# Requirements Document

## Introduction

The Repository Resurrection feature enables users to import entire legacy code repositories (VB6, Flash, Silverlight, old .NET) from Git URLs or ZIP uploads and automatically convert them to modern technologies (.NET 8, TypeScript/Angular). The system enforces strict size limits, intelligently prioritizes files for conversion, and provides comprehensive migration guidance for medium-sized projects.

## Glossary

- **Repository Resurrection System**: The complete system that imports, analyzes, converts, and packages legacy code repositories
- **Git Import Service**: The component that downloads repositories from public Git hosting platforms
- **Repository Analysis Engine**: The component that scans, categorizes, and prioritizes files for conversion
- **AI Conversion Pipeline**: The component that orchestrates batch file conversion using OpenAI
- **Migration Guide Generator**: The component that creates human-readable migration documentation
- **Project Packaging Service**: The component that creates downloadable ZIP archives of converted projects
- **Background Job Processor**: The component that manages asynchronous conversion workflows
- **Small Project**: A repository with ≤20 files and ≤5,000 total lines of code
- **Medium Project**: A repository with 21-50 files and 5,001-15,000 total lines of code
- **Legacy Technology**: VB6, ActionScript/Flash, Silverlight, or .NET Framework versions prior to .NET Core
- **Modern Technology**: .NET 8 C#, TypeScript, Angular 17, or Blazor
- **File Criticality Score**: A numeric value assigned to each file based on its importance to the application architecture

## Requirements

### Requirement 1

**User Story:** As a developer, I want to import a legacy repository from a public Git URL, so that I can resurrect my old projects without manually downloading them

#### Acceptance Criteria

1. WHEN the user provides a GitHub, GitLab, or Bitbucket repository URL, THE Git Import Service SHALL convert the URL to the appropriate ZIP archive download URL format
2. WHEN the Git Import Service attempts to download a repository, THE Git Import Service SHALL try branch names in the following order: main, master, develop
3. IF the Git Import Service receives an HTTP 404 response, THEN THE Git Import Service SHALL return an error message indicating the repository is private or not found
4. WHEN the Git Import Service successfully downloads a repository, THE Git Import Service SHALL return the ZIP file bytes and a unique repository identifier
5. THE Git Import Service SHALL complete the download operation within 60 seconds or return a timeout error

### Requirement 2

**User Story:** As a developer, I want to upload a ZIP file of my legacy project, so that I can resurrect private or local repositories

#### Acceptance Criteria

1. WHEN the user uploads a ZIP file via multipart form data, THE Repository Resurrection System SHALL validate that the file size is less than 50 megabytes
2. IF the uploaded file exceeds 50 megabytes, THEN THE Repository Resurrection System SHALL reject the upload and return an error message stating the size limit
3. WHEN the Repository Resurrection System accepts a ZIP upload, THE Repository Resurrection System SHALL save the file to a temporary folder with a GUID-based filename
4. WHEN the ZIP file is successfully saved, THE Repository Resurrection System SHALL return a unique upload identifier to the user
5. THE Repository Resurrection System SHALL support ZIP files with standard compression formats including deflate and deflate64

### Requirement 3

**User Story:** As a developer, I want the system to analyze my repository and enforce size limits, so that I know immediately if my project can be converted

#### Acceptance Criteria

1. WHEN the Repository Analysis Engine scans a repository, THE Repository Analysis Engine SHALL identify files with extensions: .vb, .bas, .cls, .frm, .as, .xaml, .cs
2. WHEN the Repository Analysis Engine scans a repository, THE Repository Analysis Engine SHALL exclude files in directories named: bin, obj, node_modules
3. WHEN the Repository Analysis Engine scans a repository, THE Repository Analysis Engine SHALL exclude files with names containing: .Designer., .generated.
4. WHEN the Repository Analysis Engine counts lines of code, THE Repository Analysis Engine SHALL count all non-empty lines in identified source files
5. IF a repository contains more than 50 files, THEN THE Repository Analysis Engine SHALL reject the repository with an error message stating the file limit
6. IF a repository contains more than 15,000 total lines of code, THEN THE Repository Analysis Engine SHALL reject the repository with an error message stating the line count limit
7. IF any single file contains more than 1,000 lines of code, THEN THE Repository Analysis Engine SHALL reject the repository with an error message identifying the oversized file
8. WHEN the Repository Analysis Engine completes analysis, THE Repository Analysis Engine SHALL return the total file count, total line count, and detected legacy technology

### Requirement 4

**User Story:** As a developer, I want the system to intelligently prioritize which files to convert, so that the most important code is converted first in medium projects

#### Acceptance Criteria

1. WHEN the Repository Analysis Engine scores files, THE Repository Analysis Engine SHALL assign a score of +100 to files containing entry points such as Main or Program methods
2. WHEN the Repository Analysis Engine scores files, THE Repository Analysis Engine SHALL assign a score of +80 to files in directories named Models or Entities
3. WHEN the Repository Analysis Engine scores files, THE Repository Analysis Engine SHALL assign a score of +60 to files in directories named Services or Business
4. WHEN the Repository Analysis Engine scores files, THE Repository Analysis Engine SHALL assign a score of +40 to files in directories named Controllers or Forms
5. WHEN the Repository Analysis Engine scores files, THE Repository Analysis Engine SHALL assign a score of +20 to files containing fewer than 100 lines of code
6. WHEN the Repository Analysis Engine scores files, THE Repository Analysis Engine SHALL assign a score of -1000 to files containing more than 1,000 lines of code
7. WHEN a repository is classified as a Medium Project, THE Repository Analysis Engine SHALL select the top 15 files by criticality score for conversion
8. WHEN the Repository Analysis Engine selects files for conversion, THE Repository Analysis Engine SHALL return a list ordered by descending criticality score

### Requirement 5

**User Story:** As a developer, I want all files in my small project to be converted automatically, so that I receive a complete modern codebase

#### Acceptance Criteria

1. WHEN a repository contains 20 or fewer files and 5,000 or fewer total lines of code, THE Repository Resurrection System SHALL classify it as a Small Project
2. WHEN the Repository Resurrection System processes a Small Project, THE AI Conversion Pipeline SHALL convert all identified source files
3. WHEN the AI Conversion Pipeline converts a Small Project, THE AI Conversion Pipeline SHALL process files in the following order: models, services, UI components
4. WHEN the AI Conversion Pipeline completes conversion of a Small Project, THE Project Packaging Service SHALL include all converted files in the output package
5. THE Repository Resurrection System SHALL NOT generate a migration guide for Small Projects

### Requirement 6

**User Story:** As a developer, I want the most critical files converted and a migration guide for my medium project, so that I can complete the conversion manually with clear guidance

#### Acceptance Criteria

1. WHEN a repository contains 21-50 files or 5,001-15,000 total lines of code, THE Repository Resurrection System SHALL classify it as a Medium Project
2. WHEN the Repository Resurrection System processes a Medium Project, THE AI Conversion Pipeline SHALL convert exactly 15 files selected by the Repository Analysis Engine
3. WHEN the AI Conversion Pipeline completes conversion of a Medium Project, THE Migration Guide Generator SHALL create a markdown migration guide
4. WHEN the Migration Guide Generator creates a guide, THE Migration Guide Generator SHALL include sections for: project summary, find-and-replace patterns, file-by-file notes, and manual steps
5. WHEN the Migration Guide Generator creates a guide, THE Migration Guide Generator SHALL limit the OpenAI request to 3,000 tokens maximum
6. WHEN the Project Packaging Service packages a Medium Project, THE Project Packaging Service SHALL include the migration guide as MIGRATION_GUIDE.md in the root directory

### Requirement 7

**User Story:** As a developer, I want VB6 code converted to modern .NET 8 C#, so that my legacy Visual Basic applications can run on current platforms

#### Acceptance Criteria

1. WHEN the AI Conversion Pipeline detects VB6 source files, THE AI Conversion Pipeline SHALL target .NET 8 C# as the output language
2. WHEN the AI Conversion Pipeline converts VB6 collections, THE AI Conversion Pipeline SHALL use C# generic List or Dictionary types
3. WHEN the AI Conversion Pipeline converts VB6 error handling, THE AI Conversion Pipeline SHALL use C# try-catch-finally blocks with structured exception handling
4. WHEN the AI Conversion Pipeline converts VB6 file I/O, THE AI Conversion Pipeline SHALL use async C# File class methods
5. WHEN the AI Conversion Pipeline converts VB6 code, THE AI Conversion Pipeline SHALL preserve all business logic and validation rules

### Requirement 8

**User Story:** As a developer, I want Flash/ActionScript code converted to TypeScript and Angular, so that my interactive applications can run in modern browsers

#### Acceptance Criteria

1. WHEN the AI Conversion Pipeline detects ActionScript source files, THE AI Conversion Pipeline SHALL target TypeScript and Angular 17 as the output technologies
2. WHEN the AI Conversion Pipeline converts ActionScript event listeners, THE AI Conversion Pipeline SHALL use Angular event binding syntax
3. WHEN the AI Conversion Pipeline converts ActionScript display objects, THE AI Conversion Pipeline SHALL use HTML5 Canvas or SVG with TypeScript
4. WHEN the AI Conversion Pipeline converts ActionScript animations, THE AI Conversion Pipeline SHALL use CSS animations or Angular animation framework
5. WHEN the AI Conversion Pipeline converts ActionScript code, THE AI Conversion Pipeline SHALL maintain the original application behavior and user interactions

### Requirement 9

**User Story:** As a developer, I want Silverlight XAML converted to Blazor or Angular, so that my rich client applications work without browser plugins

#### Acceptance Criteria

1. WHEN the AI Conversion Pipeline detects Silverlight XAML files, THE AI Conversion Pipeline SHALL target Blazor or Angular 17 as the output technology
2. WHEN the AI Conversion Pipeline converts Silverlight data binding, THE AI Conversion Pipeline SHALL use two-way binding syntax appropriate to the target framework
3. WHEN the AI Conversion Pipeline converts Silverlight commands, THE AI Conversion Pipeline SHALL use event handlers in the target framework
4. WHEN the AI Conversion Pipeline converts Silverlight layouts, THE AI Conversion Pipeline SHALL use equivalent CSS Grid or Flexbox layouts
5. WHEN the AI Conversion Pipeline converts Silverlight code, THE AI Conversion Pipeline SHALL preserve all data validation and business rules

### Requirement 10

**User Story:** As a developer, I want the AI conversion to use context from previously converted files, so that the converted code is consistent and properly integrated

#### Acceptance Criteria

1. WHEN the AI Conversion Pipeline converts a file, THE AI Conversion Pipeline SHALL include code from previously converted files in the conversion prompt
2. WHEN the AI Conversion Pipeline builds a conversion prompt, THE AI Conversion Pipeline SHALL use a maximum of 2,000 tokens for the OpenAI request
3. WHEN the AI Conversion Pipeline calls OpenAI, THE AI Conversion Pipeline SHALL use the gpt-3.5-turbo model with temperature set to 0.3
4. WHEN the AI Conversion Pipeline receives a conversion response, THE AI Conversion Pipeline SHALL extract the converted code and add it to the context for subsequent conversions
5. IF the AI Conversion Pipeline encounters a conversion failure, THEN THE AI Conversion Pipeline SHALL log the error and continue processing remaining files

### Requirement 11

**User Story:** As a developer, I want to download a complete package with my converted code, so that I can immediately start working with the modern codebase

#### Acceptance Criteria

1. WHEN the Project Packaging Service creates an output package, THE Project Packaging Service SHALL create a ZIP file with the project name followed by "-Resurrected"
2. WHEN the Project Packaging Service creates an output package, THE Project Packaging Service SHALL include a src directory containing all converted source files
3. WHEN the Project Packaging Service creates an output package, THE Project Packaging Service SHALL include a README.md file with project information
4. WHEN the Project Packaging Service creates an output package for .NET projects, THE Project Packaging Service SHALL include a .csproj file with appropriate package references
5. WHEN the Project Packaging Service creates an output package for TypeScript projects, THE Project Packaging Service SHALL include a package.json file with appropriate dependencies
6. WHEN the Project Packaging Service creates an output package, THE Project Packaging Service SHALL include a .gitignore file appropriate to the target technology
7. WHEN the user requests a download, THE Repository Resurrection System SHALL return the packaged ZIP file with appropriate content-type headers

### Requirement 12

**User Story:** As a developer, I want to track the progress of my conversion in real-time, so that I know the system is working and how long to wait

#### Acceptance Criteria

1. WHEN the Background Job Processor starts a conversion job, THE Background Job Processor SHALL set the initial status to "downloading" with 0% progress
2. WHEN the Background Job Processor completes downloading, THE Background Job Processor SHALL update the status to "extracting" with 20% progress
3. WHEN the Background Job Processor completes extraction, THE Background Job Processor SHALL update the status to "analyzing" with 40% progress
4. WHEN the Background Job Processor completes analysis, THE Background Job Processor SHALL update the status to "converting" with 60% progress
5. WHEN the Background Job Processor completes conversion, THE Background Job Processor SHALL update the status to "packaging" with 80% progress
6. WHEN the Background Job Processor completes packaging, THE Background Job Processor SHALL update the status to "completed" with 100% progress
7. IF the Background Job Processor encounters an error, THEN THE Background Job Processor SHALL update the status to "failed" with an error message
8. WHEN the user polls for status, THE Repository Resurrection System SHALL return the current status, progress percentage, status message, and result data if completed

### Requirement 13

**User Story:** As a developer, I want clear error messages when my repository cannot be converted, so that I understand what needs to be fixed

#### Acceptance Criteria

1. WHEN the Repository Resurrection System rejects a repository, THE Repository Resurrection System SHALL return an error message that clearly states the reason for rejection
2. WHEN a repository exceeds file count limits, THE Repository Resurrection System SHALL return an error message stating the actual file count and the maximum allowed
3. WHEN a repository exceeds line count limits, THE Repository Resurrection System SHALL return an error message stating the actual line count and the maximum allowed
4. WHEN a Git URL is invalid or inaccessible, THE Repository Resurrection System SHALL return an error message indicating the repository is private, not found, or the URL is malformed
5. THE Repository Resurrection System SHALL NOT expose internal stack traces or technical implementation details in user-facing error messages

### Requirement 14

**User Story:** As a system administrator, I want temporary files cleaned up after conversion, so that disk space is not wasted

#### Acceptance Criteria

1. WHEN the Background Job Processor completes a conversion job with status "completed", THE Background Job Processor SHALL delete all temporary files associated with that job
2. WHEN the Background Job Processor completes a conversion job with status "failed", THE Background Job Processor SHALL delete all temporary files associated with that job
3. WHEN the Background Job Processor deletes temporary files, THE Background Job Processor SHALL remove the uploaded ZIP file, extracted directory, and intermediate conversion artifacts
4. THE Background Job Processor SHALL retain the final packaged ZIP file for download until explicitly deleted by the user or system cleanup policy
5. WHEN temporary file cleanup fails, THE Background Job Processor SHALL log the error but SHALL NOT fail the overall conversion job

### Requirement 15

**User Story:** As a project manager, I want conversion costs to remain between $2-7 per repository, so that the service is economically viable

#### Acceptance Criteria

1. WHEN the AI Conversion Pipeline processes a Small Project, THE AI Conversion Pipeline SHALL consume no more than $7 worth of OpenAI API credits
2. WHEN the AI Conversion Pipeline processes a Medium Project, THE AI Conversion Pipeline SHALL consume no more than $7 worth of OpenAI API credits
3. WHEN the AI Conversion Pipeline makes OpenAI requests, THE AI Conversion Pipeline SHALL use token limits and model selection that optimize for cost efficiency
4. THE Repository Resurrection System SHALL log the total token usage for each conversion job for cost tracking purposes
5. THE Repository Resurrection System SHALL provide cost estimates to administrators based on repository size before starting conversion
