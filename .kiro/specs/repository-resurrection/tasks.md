# Implementation Plan

- [x] 1. Create core data models and database schema












  - Create RepositoryProject model extending existing Project model with repository-specific fields
  - Create RepositoryFile model for tracking individual files in repositories
  - Add ProjectSize and RepositorySource enums
  - Update AppDbContext with new DbSets and relationships
  - Create database migration
  - _Requirements: 1.4, 2.4, 3.8_

- [x] 2. Implement Git Repository Service







  - [x] 2.1 Create IGitRepositoryService interface and implementation


    - Implement URL validation and whitelisting for GitHub, GitLab, Bitbucket
    - Create URL-to-ZIP conversion logic for each Git platform
    - Implement branch fallback logic (main → master → develop)
    - Add HttpClient configuration with timeout and retry policies
    - _Requirements: 1.1, 1.2, 1.3_
  
  - [x] 2.2 Implement repository download functionality


    - Create DownloadRepositoryAsync method with cancellation support
    - Handle HTTP 404 responses with user-friendly error messages
    - Implement download timeout (60 seconds)
    - Add logging for download operations
    - _Requirements: 1.3, 1.4, 1.5_

- [x] 3. Implement File Upload Service enhancements

























  - [x] 3.1 Add ZIP upload validation




    - Implement 20MB size limit validation
    - Add ZIP file format validation
    - Create GUID-based temporary file naming
    - Implement secure file path validation (prevent directory traversal)
    - _Requirements: 2.1, 2.2, 2.5_
  



  - [x] 3.2 Create ZIP extraction functionality


    - Implement ZIP extraction to temporary directory
    - Add zip bomb protection (limit extraction size)
    - Validate extracted file paths
    - Handle extraction errors gracefully
    - _Requirements: 2.3, 2.4_

- [x] 4. Implement Repository Analysis Service










  - [x] 4.1 Create file scanning and filtering logic

    - Implement recursive directory scanning
    - Add file extension filtering (.vb, .bas, .cls, .frm, .as, .xaml, .cs)
    - Implement directory exclusion (bin, obj, node_modules)
    - Add filename pattern exclusion (.Designer., .generated.)
    - _Requirements: 3.1, 3.2, 3.3_
  

  - [x] 4.2 Implement line counting and limit validation


    - Create line counting logic (non-empty lines)
    - Implement file count validation (max 50 files)
    - Implement total LOC validation (max 15,000 LOC)
    - Implement single file LOC validation (max 1,000 LOC)
    - Create detailed validation error messages
    - _Requirements: 3.4, 3.5, 3.6, 3.7, 13.2, 13.3_
  


  - [x] 4.3 Implement file criticality scoring algorithm

    - Create entry point detection (+100 score)
    - Implement directory-based scoring (Models: +80, Services: +60, Controllers: +40)
    - Add size-based scoring (<100 LOC: +20, >1000 LOC: -1000)
    - Implement file sorting by criticality score
    - Create top 15 file selection for medium projects
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 4.8_
  

  - [x] 4.4 Implement technology detection

    - Create technology detection based on file extensions and content
    - Return RepositoryAnalysisResult with all analysis data
    - _Requirements: 3.8_

- [x] 5. Implement Repository Conversion Service





  - [x] 5.1 Create batch conversion orchestration


    - Implement file ordering logic (models → services → UI)
    - Create conversion context management
    - Implement progress reporting with IProgress<T>
    - Add cancellation token support
    - _Requirements: 5.3, 10.1_
  
  - [x] 5.2 Implement contextual prompt building

    - Create prompt builder that includes previous conversion results
    - Implement token limit management (max 2000 tokens)
    - Add context prioritization (recent and related files)
    - Build technology-specific prompts using existing AiConversionService patterns
    - _Requirements: 10.1, 10.2, 10.3_
  
  - [x] 5.3 Implement file conversion with error handling

    - Integrate with existing IAiConversionService
    - Use gpt-3.5-turbo model with temperature 0.3
    - Implement error logging and continuation on failure
    - Track token usage and estimated costs
    - Store converted code and add to context
    - _Requirements: 10.3, 10.4, 10.5, 15.3_
  
  - [x] 5.4 Implement technology-specific conversion logic

    - Ensure VB6 → .NET 8 C# conversion preserves business logic
    - Ensure ActionScript → TypeScript/Angular conversion maintains behavior
    - Ensure Silverlight → Blazor/Angular conversion preserves data binding
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 8.1, 8.2, 8.3, 8.4, 8.5, 9.1, 9.2, 9.3, 9.4, 9.5_

- [x] 6. Implement Migration Guide Service






  - [x] 6.1 Create migration guide generation

    - Build OpenAI prompt for guide generation with 3000 token limit
    - Include project summary, converted files list, and technology mapping
    - Generate find-and-replace patterns section
    - Create file-by-file notes from conversion results
    - Generate manual steps section for unconverted files
    - List dependencies and breaking changes
    - _Requirements: 6.3, 6.4, 6.5_
  

  - [x] 6.2 Format migration guide as markdown

    - Create structured markdown with proper sections
    - Include statistics (files converted, estimated completion time)
    - Add warnings and breaking changes sections
    - _Requirements: 6.3, 6.4_

- [x] 7. Enhance Project Packaging Service






  - [x] 7.1 Create repository package structure

    - Implement directory structure creation (src/, Models/, Services/, Controllers/)
    - Place converted files in appropriate directories
    - Create project name with "-Resurrected" suffix
    - _Requirements: 11.1, 11.2_
  

  - [x] 7.2 Generate project configuration files


    - Create .csproj file for .NET projects with package references
    - Create package.json for TypeScript/Angular projects with dependencies
    - Generate .gitignore appropriate to target technology
    - Create README.md with project information and setup instructions
    - _Requirements: 11.3, 11.4, 11.5, 11.6_
  



  - [x] 7.3 Include migration guide for medium projects


    - Add MIGRATION_GUIDE.md to package root for medium projects
    - Ensure guide is excluded for small projects
    - _Requirements: 6.6_
  







  - [x] 7.4 Create final ZIP package


    - Implement ZIP creation with proper structure
    - Return byte array for download
    - _Requirements: 11.7_

- [x] 8. Implement Background Job Processor






  - [x] 8.1 Create job state management

    - Create RepositoryJob model with all status fields
    - Implement in-memory ConcurrentDictionary for job storage
    - Create job CRUD operations (Add, Get, Update, Remove)
    - _Requirements: 12.1_
  

  - [x] 8.2 Implement job processing workflow

    - Create ProcessRepositoryJobAsync method with full workflow
    - Implement progress updates at each stage (20%, 40%, 60%, 80%, 90%, 100%)
    - Update status messages for each stage
    - Handle job completion and failure states
    - _Requirements: 12.2, 12.3, 12.4, 12.5, 12.6, 12.7_
  
  - [x] 8.3 Implement temporary file cleanup



    - Delete uploaded ZIP files after processing
    - Remove extracted directories after completion
    - Clean up intermediate conversion artifacts
    - Retain final packaged ZIP for download
    - Handle cleanup errors gracefully
    - _Requirements: 14.1, 14.2, 14.3, 14.4, 14.5_
  
  - [x] 8.4 Add background task execution


    - Use Task.Run for async processing
    - Implement proper service scope management with IServiceScopeFactory
    - Add comprehensive error handling and logging
    - _Requirements: 12.8_

- [x] 9. Create Repository Controller API endpoints




  - [x] 9.1 Implement POST /api/repository/import endpoint

    - Accept GitImportRequest with URL
    - Validate Git URL format
    - Create new RepositoryProject record
    - Start background job for Git download
    - Return repository_id and initial status
    - _Requirements: 1.1, 1.4_
  

  - [x] 9.2 Implement POST /api/repository/upload endpoint


    - Accept multipart file upload
    - Validate file size (50MB limit)
    - Create new RepositoryProject record
    - Start background job for ZIP extraction
    - Return repository_id and initial status
    - _Requirements: 2.1, 2.2, 2.4_
  



  - [x] 9.3 Implement GET /api/repository/status/{id} endpoint
    - Retrieve job from in-memory store
    - Return current status, progress, message, and results
    - Handle not found cases
    - _Requirements: 12.8_
  



  - [x] 9.4 Implement GET /api/repository/download/{id} endpoint

    - Retrieve completed job package
    - Return ZIP file with appropriate headers
    - Handle not found and incomplete job cases
    - _Requirements: 11.7_

- [x] 10. Implement error handling and validation






  - [x] 10.1 Create custom exception types

    - Create RepositoryException with user-friendly messages
    - Add details and suggestion fields
    - Implement exception handling in all services
    - _Requirements: 13.1, 13.5_
  

  - [x] 10.2 Add global exception handler

    - Enhance existing exception middleware
    - Return structured error responses
    - Log errors without exposing stack traces
    - _Requirements: 13.5_
  

  - [x] 10.3 Implement validation error responses

    - Create detailed error messages for size limit violations
    - Add helpful suggestions for rejected repositories
    - Format Git URL errors clearly
    - _Requirements: 13.2, 13.3, 13.4_

- [x] 11. Create Repository Import frontend component






  - [x] 11.1 Create component structure with tabs

    - Create RepositoryImportComponent with Angular Material tabs
    - Add Git Import tab with URL input field
    - Add ZIP Upload tab with drag-drop zone
    - Implement tab switching and styling
    - _Requirements: 1.1, 2.1_
  
  - [x] 11.2 Implement Git import functionality

    - Add URL input validation
    - Create importFromGit method calling RepositoryService
    - Navigate to status page on success
    - Display error messages on failure
    - _Requirements: 1.1_
  
  - [x] 11.3 Implement ZIP upload functionality

    - Create drag-drop directive
    - Implement file selection via browse button
    - Add 50MB size validation
    - Create uploadZip method calling RepositoryService
    - Navigate to status page on success
    - _Requirements: 2.1, 2.2_
  

  - [x] 11.4 Add limitations notice UI





    - Display size limits prominently
    - Style as orange warning banner
    - Include all limit details (files, LOC, file size)
    - _Requirements: 3.5, 3.6, 3.7_

- [x] 12. Create Conversion Status frontend component






  - [x] 12.1 Implement status polling

    - Create ConversionStatusComponent
    - Implement 2-second polling with RxJS interval
    - Stop polling when status is completed or failed
    - Handle component cleanup
    - _Requirements: 12.8_
  

  - [x] 12.2 Create status display UI

    - Display status emoji based on current status
    - Show status title and badge
    - Render progress bar with glow effect during conversion
    - Display status message
    - _Requirements: 12.1, 12.2, 12.3, 12.4, 12.5, 12.6_
  

  - [x] 12.3 Implement results display

    - Show conversion statistics (files analyzed, files converted, cost)
    - Add download button for completed conversions
    - Display file tree visualization component
    - _Requirements: 5.4, 15.1, 15.2_
  

  - [x] 12.4 Implement error display

    - Show error icon and message for failed conversions
    - Display list of validation errors
    - Provide helpful suggestions
    - _Requirements: 12.7, 13.1_

- [-] 13. Create File Tree Visualization component




  - [x] 13.1 Build tree data structure

    - Create TreeNode interface
    - Convert flat file list to hierarchical tree
    - Build both original and converted trees
    - Track conversion status per file
    - _Requirements: 4.8, 5.4_
  

  - [x] 13.2 Implement tree rendering

    - Create TreeNodeComponent for recursive rendering
    - Display file/folder icons
    - Show line counts for files
    - Display conversion status icons (check, skip)
    - _Requirements: 5.4, 6.1_
  

  - [x] 13.3 Add side-by-side visualization

    - Create two-column layout
    - Show original structure on left
    - Show converted structure on right
    - Highlight converted files in green
    - Grey out skipped files
    - _Requirements: 5.4, 6.1_
  

  - [x] 13.4 Implement tombstone rising animation





    - Add CSS animation for converted files
    - Trigger animation as files complete conversion
    - Use theme-appropriate timing and easing
    - _Requirements: 5.4_

- [x] 14. Create Repository Service for frontend




  - [x] 14.1 Implement API communication methods


    - Create importFromGit method
    - Create uploadZip method with FormData
    - Create getJobStatus method
    - Create downloadPackage method with blob handling
    - _Requirements: 1.4, 2.4, 12.8, 11.7_
  

  - [x] 14.2 Add HTTP error handling

    - Create HTTP interceptor for repository errors
    - Parse structured error responses
    - Display toast notifications with details and suggestions
    - _Requirements: 13.1, 13.5_

- [x] 15. Add routing and navigation





  - Create route for /repository/import
  - Create route for /repository/status/:id
  - Add navigation from import to status page
  - Add link to repository import from main dashboard
  - _Requirements: 1.4, 2.4_

- [x] 16. Implement cost tracking and logging








  - [x] 16.1 Add token usage tracking

    - Log OpenAI token usage for each conversion
    - Calculate estimated cost based on token usage
    - Store cost data in RepositoryProject
    - _Requirements: 15.4_
  

  - [x] 16.2 Add cost validation

    - Verify small project conversions stay under $7
    - Verify medium project conversions stay under $7
    - Log warnings if costs exceed targets
    - _Requirements: 15.1, 15.2, 15.5_
  

  - [x] 16.3 Display cost information

    - Show estimated cost in status component
    - Include cost in completion statistics
    - _Requirements: 15.1, 15.2_




- [x] 17. Add configuration and deployment setup


  - Add Repository section to appsettings.json with all limits
  - Configure OpenAI settings for repository conversion
  - Set up temporary directory configuration
  - Add rate limiting configuration
  - Update README with repository resurrection feature documentation
  - _Requirements: 3.5, 3.6, 3.7, 15.3_

- [ ]* 18. Create integration tests
  - [ ]* 18.1 Test Git import flow
    - Test successful Git repository import
    - Test invalid URL handling
    - Test private repository handling (404)
    - Test branch fallback logic
    - _Requirements: 1.1, 1.2, 1.3_
  
  - [ ]* 18.2 Test ZIP upload flow
    - Test successful ZIP upload
    - Test file size limit enforcement
    - Test ZIP extraction
    - _Requirements: 2.1, 2.2, 2.3_
  
  - [ ]* 18.3 Test repository analysis
    - Test file counting and filtering
    - Test LOC counting
    - Test limit validation and rejection
    - Test file prioritization
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 4.1-4.8_
  
  - [ ]* 18.4 Test conversion workflows
    - Test small project full conversion
    - Test medium project partial conversion with guide
    - Test error handling and continuation
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 6.1, 6.2_
  
  - [ ]* 18.5 Test packaging and download
    - Test package structure creation
    - Test project file generation
    - Test download endpoint
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 11.6, 11.7_
