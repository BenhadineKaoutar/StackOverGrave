# RIP Date and Technology Icon Fix

## Problems
1. **RIP Date always shows 2025**: The tombstone cards were displaying the upload date (2025) instead of when the technology was actually deprecated
2. **Tech icon shows "?"**: The technology field was showing as "Unknown" because it wasn't being properly stored and retrieved

## Root Causes
1. The frontend was using `uploadedAt` (when user uploaded the file) instead of the technology's actual deprecation date
2. The `DeprecatedDate` from the death certificate wasn't being stored in the database
3. The backend wasn't returning the deprecated date in the graveyard API

## Solution
Added a `DeprecatedDate` field to the Project model and updated the entire flow to store and display it.

## Changes Made

### Backend Changes

#### 1. Project Model (`backend/Models/Project.cs`)
Added nullable `DeprecatedDate` field:
```csharp
public class Project
{
    // ... existing fields
    public DateTime? DeprecatedDate { get; set; }
}
```

#### 2. Analyze Endpoint (`backend/Controllers/ProjectController.cs`)
Store the deprecated date when analyzing:
```csharp
[HttpGet("analyze/{id}")]
public async Task<IActionResult> Analyze(Guid id)
{
    var certificate = await _techDetection.AnalyzeFileAsync(project.FilePath);
    
    project.Technology = certificate.Technology;
    project.LinesOfCode = certificate.FileStats.LinesOfCode;
    project.DeprecatedDate = certificate.DeprecatedDate;  // NEW
    await _context.SaveChangesAsync();
    
    return Ok(certificate);
}
```

#### 3. Graveyard Endpoint (`backend/Controllers/ProjectController.cs`)
Include deprecated date in response:
```csharp
[HttpGet("graveyard")]
public async Task<IActionResult> GetGraveyard()
{
    var projects = await _context.Projects
        .Select(p => new
        {
            id = p.Id,
            originalFilename = p.OriginalFilename,
            technology = p.Technology.ToString(),
            uploadedAt = p.UploadedAt,
            status = p.Status.ToString(),
            fileSize = p.FileSize,
            linesOfCode = p.LinesOfCode,
            deprecatedDate = p.DeprecatedDate  // NEW
        })
        .ToListAsync();
    
    return Ok(projects);
}
```

#### 4. Database Migration
Created migration: `20251205180409_AddDeprecatedDateToProject.cs`

### Frontend Changes

#### 1. Project Model (`frontend/src/app/models/project.model.ts`)
Added optional `deprecatedDate` field:
```typescript
export interface Project {
  id: string;
  originalFilename: string;
  technology: TechnologyType;
  uploadedAt: Date;
  status: ProjectStatus;
  fileSize: number;
  linesOfCode?: number;
  deprecatedDate?: Date;  // NEW
}
```

Also fixed DeathCertificate to use string types (as returned by backend):
```typescript
export interface DeathCertificate {
  technology: string;  // Changed from TechnologyType
  deprecatedDate: string;  // Changed from Date
  // ... other fields
}
```

#### 2. Graveyard Dashboard (`graveyard-dashboard.component.ts`)
Store deprecated date when analyzing:
```typescript
const newProject: Project = {
  id: event.id,
  originalFilename: file.name,
  technology: this.mapTechnology(certificate.technology),
  uploadedAt: new Date(),
  status: ProjectStatus.Uploaded,
  fileSize: file.size,
  linesOfCode: certificate.fileStats.linesOfCode,
  deprecatedDate: new Date(certificate.deprecatedDate)  // NEW
};
```

Map deprecated date when loading projects:
```typescript
loadProjects(): void {
  this.projectService.getProjects().subscribe({
    next: (projects) => {
      const mappedProjects = projects.map(p => ({
        ...p,
        technology: this.mapTechnology(p.technology),
        status: this.mapStatus(p.status),
        uploadedAt: new Date(p.uploadedAt),
        deprecatedDate: p.deprecatedDate ? new Date(p.deprecatedDate) : undefined  // NEW
      }));
      this.projects.set(mappedProjects);
    }
  });
}
```

#### 3. Tombstone Card (`tombstone-card.component.html`)
Use deprecated date instead of upload date:
```html
<!-- Before -->
<p class="rip-date">RIP {{ project.uploadedAt | date:'yyyy' }}</p>

<!-- After -->
<p class="rip-date">RIP {{ (project.deprecatedDate || project.uploadedAt) | date:'yyyy' }}</p>
```

## Technology Deprecation Dates

The backend's `TechnologyDetectionService` returns these dates:

| Technology | Deprecated Date | Display |
|------------|----------------|---------|
| VB6 | April 8, 2008 | RIP 2008 |
| ActionScript (Flash) | December 31, 2020 | RIP 2020 |
| Silverlight | October 12, 2021 | RIP 2021 |
| .NET Framework 4.x | Current date | RIP 2025 |

## Testing

After restarting the backend and running the migration:

1. **New uploads**: Upload a VB6 file → Should show "RIP 2008" and 🔷 icon
2. **Existing projects**: Reload the page → Should show correct deprecated dates
3. **Technology icons**: Each tech should show its proper icon:
   - VB6: 🔷
   - ActionScript: ⚡
   - Silverlight: 🌙
   - .NET Framework: 🔵
   - Unknown: ❓

## Migration Required

Run this command to apply the database migration:
```bash
cd backend
dotnet ef database update
```

Or restart the backend - it will auto-apply migrations on startup.
