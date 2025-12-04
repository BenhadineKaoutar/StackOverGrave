# Graveyard Dashboard UI Spec

## Overview
Main dashboard displaying uploaded projects as animated tombstone cards in a spooky graveyard theme.

## Layout

### Grid Structure
- Responsive grid: 1 column (mobile), 2 columns (tablet), 3-4 columns (desktop)
- Card spacing: 24px gap
- Max width: 1400px, centered
- Padding: 32px

### Header Section
- Title: "StackOverGrave" in gothic font (Nosifer or Creepster)
- Subtitle: "Where Legacy Code Rests in Peace"
- Upload button: Prominent, glowing green
- Filter chips: All, VB6, Flash, Silverlight, .NET Framework

## Tombstone Card Component

### Card Structure
```typescript
interface TombstoneCard {
  id: string;
  technology: TechnologyType;
  filename: string;
  fileSize: number;
  uploadedAt: Date;
  status: 'Dead' | 'Resurrecting' | 'Alive' | 'Failed';
  linesOfCode?: number;
}
```

### Visual Design
- Shape: Rounded rectangle with tombstone silhouette
- Background: Dark gray (#2a2a2a) with stone texture
- Border: 2px solid #444
- Shadow: 0 8px 16px rgba(0, 255, 65, 0.1)
- Hover: Lift effect (translateY(-8px)), glow intensifies

### Card Content Layout
```
┌─────────────────────────┐
│   [Technology Icon]     │
│                         │
│   Filename.ext          │
│   RIP 2024              │
│                         │
│   [Status Badge]        │
│                         │
│   📄 1.2 MB | 450 LOC   │
└─────────────────────────┘
```

### Technology Icons
- VB6: Classic VB icon or "VB6" text in retro font
- Flash: Lightning bolt or Flash logo
- Silverlight: Silverlight logo (grayscale)
- .NET Framework: .NET logo with "4.x" badge

### Status Badges
- **Dead** (uploaded): Gray skull icon, "Awaiting Resurrection"
- **Resurrecting** (processing): Animated green glow, "Resurrecting..."
- **Alive** (completed): Green checkmark, "Resurrected"
- **Failed** (error): Red X, "Resurrection Failed"

## Animations

### Tombstone Appear (on upload)
```css
@keyframes tombstone-rise {
  0% {
    opacity: 0;
    transform: translateY(100px) scale(0.8);
  }
  60% {
    transform: translateY(-10px) scale(1.05);
  }
  100% {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
```
Duration: 800ms, easing: cubic-bezier(0.34, 1.56, 0.64, 1)

### Crack Animation (during processing)
- SVG overlay with crack pattern
- Crack grows from top to bottom over 2 seconds
- Repeat 3 times during processing
- Green light emanates from crack

```typescript
// Angular animation
trigger('crack', [
  state('idle', style({ opacity: 0 })),
  state('cracking', style({ opacity: 1 })),
  transition('idle => cracking', [
    animate('2s', keyframes([
      style({ clipPath: 'inset(0 0 100% 0)', offset: 0 }),
      style({ clipPath: 'inset(0 0 50% 0)', offset: 0.5 }),
      style({ clipPath: 'inset(0 0 0 0)', offset: 1 })
    ]))
  ])
])
```

### Glow Effect (on completion)
```css
@keyframes success-glow {
  0%, 100% {
    box-shadow: 0 0 20px rgba(0, 255, 65, 0.5);
  }
  50% {
    box-shadow: 0 0 40px rgba(0, 255, 65, 0.8);
  }
}
```
Duration: 2s, infinite loop for 5 seconds, then fade out

### Hover Interaction
- Lift: translateY(-8px)
- Glow: box-shadow intensity increases
- Cursor: pointer
- Transition: 300ms ease-out

## Background Effects

### Fog Overlay
```css
.fog {
  position: fixed;
  top: 0;
  left: 0;
  width: 200%;
  height: 100%;
  background: linear-gradient(
    90deg,
    transparent 0%,
    rgba(255, 255, 255, 0.03) 25%,
    rgba(255, 255, 255, 0.05) 50%,
    rgba(255, 255, 255, 0.03) 75%,
    transparent 100%
  );
  animation: fog-drift 20s linear infinite;
  pointer-events: none;
}

@keyframes fog-drift {
  0% { transform: translateX(-50%); }
  100% { transform: translateX(0); }
}
```

### Floating Particles (ghosts)
- Use particles.js or custom Canvas
- 20-30 small white particles
- Slow upward drift with random horizontal movement
- Opacity: 0.1-0.3
- Size: 2-6px

### Flickering Candles (optional)
- Small candle icons in corners
- CSS animation for flame flicker
```css
@keyframes flicker {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.8; }
  75% { opacity: 0.9; }
}
```

## Filters

### Filter Chips
- Horizontal scrollable row below header
- Active filter: Green background (#00ff41), black text
- Inactive: Dark gray background, white text
- Click to toggle
- Show count: "VB6 (3)"

### Filter Logic
- "All" shows everything
- Technology filters are additive (OR logic)
- Animate filtered-out cards: fade out + scale down
- Animate filtered-in cards: fade in + scale up

## Empty States

### No Projects Yet
```
     👻
  "So Empty..."
  
Upload your first legacy code
to begin the resurrection!

[Upload File Button]
```

### No Results After Filter
```
     🪦
  "Nothing Here"
  
No projects match this filter.
Try selecting a different technology.
```

## Interactions

### Click Tombstone
- Navigate to code comparison viewer
- Route: `/project/{id}`
- Smooth transition animation

### Right-Click Menu (future)
- Download converted code
- Delete project
- View death certificate
- Share resurrection

## Responsive Behavior

### Mobile (< 768px)
- 1 column grid
- Larger cards (full width - 32px padding)
- Simplified animations (performance)
- Sticky header with filters

### Tablet (768px - 1024px)
- 2 column grid
- Medium-sized cards

### Desktop (> 1024px)
- 3-4 column grid (depending on screen width)
- Full animations enabled
- Hover effects active

## Performance Optimizations

### Virtual Scrolling
- If > 50 projects, use CDK virtual scroll
- Render only visible cards + buffer

### Animation Performance
- Use `transform` and `opacity` only (GPU accelerated)
- Disable animations on low-end devices (check `prefers-reduced-motion`)
- Lazy load particle effects

### Image Optimization
- Use SVG for icons (scalable, small)
- Lazy load background textures
- Compress any raster images

## Accessibility

### Keyboard Navigation
- Tab through tombstone cards
- Enter to open project
- Arrow keys to navigate grid

### Screen Readers
- Announce status changes: "Project X is now resurrecting"
- Descriptive labels: "VB6 project, Calculator.vb, 450 lines, uploaded 2 hours ago"
- Skip to content link

### Color Contrast
- Ensure text meets WCAG AA (4.5:1 for normal text)
- Don't rely solely on color for status (use icons too)

## Component Structure

```typescript
@Component({
  selector: 'app-graveyard-dashboard',
  standalone: true,
  imports: [CommonModule, TombstoneCardComponent, FilterChipsComponent],
  templateUrl: './graveyard-dashboard.component.html',
  styleUrls: ['./graveyard-dashboard.component.scss']
})
export class GraveyardDashboardComponent implements OnInit {
  projects$ = this.projectService.getProjects();
  selectedFilters = signal<TechnologyType[]>([]);
  
  filteredProjects = computed(() => {
    const filters = this.selectedFilters();
    return this.projects().filter(p => 
      filters.length === 0 || filters.includes(p.technology)
    );
  });
  
  onUpload(file: File) { /* ... */ }
  onFilterChange(filters: TechnologyType[]) { /* ... */ }
  onTombstoneClick(projectId: string) { /* ... */ }
}
```

## Testing Requirements
- Unit tests for filter logic
- Component tests for animations
- E2E tests for upload flow
- Visual regression tests for theme
- Accessibility audit with axe-core
