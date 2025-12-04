# Code Comparison Viewer Spec

## Overview
Split-pane component for viewing original legacy code alongside converted modern code with migration notes.

## Layout Structure

### Three-Pane Layout
```
┌─────────────────────────────────────────┐
│  Header: filename.ext → filename.cs     │
├──────────────┬──────────────────────────┤
│              │                          │
│   Original   │   Converted              │
│   Legacy     │   Modern                 │
│   Code       │   Code                   │
│              │                          │
├──────────────┴──────────────────────────┤
│   Autopsy Report (Migration Notes)      │
└─────────────────────────────────────────┘
```

### Responsive Behavior
- Desktop: Side-by-side panes (50/50 split)
- Tablet: Side-by-side with adjustable splitter
- Mobile: Stacked vertically with tabs

## Code Panes

### Monaco Editor Integration
```typescript
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';

editorOptions = {
  theme: 'vs-dark',
  language: 'csharp', // or 'vb', 'typescript', 'xml'
  readOnly: true,
  minimap: { enabled: true },
  fontSize: 14,
  lineNumbers: 'on',
  scrollBeyondLastLine: false
};
```

### Syntax Highlighting Languages
- VB6: `vb`
- ActionScript: `actionscript` or `typescript`
- C#: `csharp`
- TypeScript: `typescript`
- XAML: `xml`
- HTML: `html`

### Line Number Mapping
- Hover on line in original → Highlight equivalent in converted
- Click line → Jump to corresponding line
- Store mapping: `Map<originalLine, convertedLine>`

## Autopsy Report Section

### Content Structure
```typescript
interface AutopsyReport {
  migrationNotes: string[];
  dependencies: string[];
  breakingChanges: string[];
  warnings: string[];
}
```

### Visual Design
- Collapsible sections with icons
- Color coding:
  - Notes: Blue info icon
  - Dependencies: Green package icon
  - Breaking Changes: Orange warning icon
  - Warnings: Red alert icon

## Component Implementation

```typescript
@Component({
  selector: 'app-code-viewer',
  standalone: true,
  template: `
    <div class="viewer-container">
      <header class="viewer-header">
        <h2>{{ originalFilename }} → {{ convertedFilename }}</h2>
        <button (click)="download()">Download</button>
      </header>
      
      <div class="code-panes">
        <div class="pane original">
          <h3>Original ({{ sourceTech }})</h3>
          <ngx-monaco-editor 
            [options]="originalOptions"
            [(ngModel)]="originalCode">
          </ngx-monaco-editor>
        </div>
        
        <div class="pane converted">
          <h3>Converted ({{ targetTech }})</h3>
          <ngx-monaco-editor 
            [options]="convertedOptions"
            [(ngModel)]="convertedCode">
          </ngx-monaco-editor>
        </div>
      </div>
      
      <div class="autopsy-report">
        <h3>Autopsy Report</h3>
        <!-- Report sections -->
      </div>
    </div>
  `
})
export class CodeViewerComponent {}
```
