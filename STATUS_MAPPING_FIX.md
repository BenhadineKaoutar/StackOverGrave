# Project Status Mapping Fix

## Problem
After successfully resurrecting a file, when reloading the page, the project status shows as "Unknown" instead of "Completed". This prevents users from accessing the resurrected code.

## Root Cause
The backend was serializing the `ProjectStatus` enum as an integer (0, 1, 2, 3) instead of as a string ("Uploaded", "Processing", "Completed", "Failed"). When the frontend received these numeric values, they didn't match the expected string enum values, resulting in "Unknown" status.

## Solution
1. **Backend**: Modified the `/api/graveyard` endpoint to explicitly serialize enum values as strings
2. **Frontend**: Added proper mapping functions to convert string status values to the `ProjectStatus` enum

## Changes Made

### Backend (`ProjectController.cs`)

Modified the `GetGraveyard()` endpoint to return a DTO with explicit string conversion:

```csharp
[HttpGet("graveyard")]
public async Task<IActionResult> GetGraveyard()
{
    var projects = await _context.Projects
        .OrderByDescending(p => p.UploadedAt)
        .Take(100)
        .Select(p => new
        {
            id = p.Id,
            originalFilename = p.OriginalFilename,
            technology = p.Technology.ToString(),  // Convert enum to string
            uploadedAt = p.UploadedAt,
            status = p.Status.ToString(),          // Convert enum to string
            fileSize = p.FileSize,
            linesOfCode = p.LinesOfCode
        })
        .ToListAsync();

    return Ok(projects);
}
```

### Frontend (`graveyard-dashboard.component.ts`)

Added `mapStatus()` method and updated `loadProjects()` to properly map the status:

```typescript
loadProjects(): void {
  this.projectService.getProjects().subscribe({
    next: (projects) => {
      const mappedProjects = projects.map(p => ({
        ...p,
        technology: this.mapTechnology(p.technology),
        status: this.mapStatus(p.status),           // Map status string to enum
        uploadedAt: new Date(p.uploadedAt)
      }));
      this.projects.set(mappedProjects);
    }
  });
}

private mapStatus(status: string): ProjectStatus {
  switch (status) {
    case 'Uploaded': return ProjectStatus.Uploaded;
    case 'Processing': return ProjectStatus.Processing;
    case 'Completed': return ProjectStatus.Completed;
    case 'Failed': return ProjectStatus.Failed;
    default: return ProjectStatus.Uploaded;
  }
}
```

## How It Works

1. Backend serializes enum values as strings: `"Completed"` instead of `2`
2. Frontend receives the string value
3. `mapStatus()` converts the string to the proper `ProjectStatus` enum value
4. Tombstone card displays correct status icon and text
5. Clicking on completed projects now works correctly

## Status Values

| Backend Enum | Serialized String | Frontend Enum | Display Text |
|--------------|-------------------|---------------|--------------|
| Uploaded (0) | "Uploaded" | ProjectStatus.Uploaded | "Awaiting Resurrection" |
| Processing (1) | "Processing" | ProjectStatus.Processing | "Resurrecting..." |
| Completed (2) | "Completed" | ProjectStatus.Completed | "Resurrected" |
| Failed (3) | "Failed" | ProjectStatus.Failed | "Resurrection Failed" |

## Testing

After restarting the backend:
1. Upload and resurrect a file
2. Refresh the page
3. The project should show "Resurrected" status (✅)
4. Clicking on it should navigate to the code viewer
