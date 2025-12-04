# Starting the Frontend

## Quick Start

```bash
cd frontend
npm install
npm start
```

The app will open automatically at `http://localhost:4200`

## What You'll See

1. **Graveyard Dashboard** - Main page with:
   - Spooky fog overlay drifting across the screen
   - Floating ghost particles
   - File upload zone with drag-and-drop
   - Filter chips for different technologies
   - Tombstone cards with animations

2. **Tombstone Cards** - Each card shows:
   - Technology icon
   - Filename
   - Upload date
   - Status badge (Dead, Resurrecting, Alive, Failed)
   - File stats
   - Crack animation when processing
   - Glow effect when completed

3. **Code Viewer** - Click any tombstone to see:
   - Side-by-side code comparison
   - Original legacy code on left
   - Converted modern code on right
   - Autopsy report with tabs:
     - Migration notes
     - Dependencies
     - Breaking changes
     - Warnings

## Features Implemented

### Animations
- ✅ Tombstone rise animation on upload
- ✅ Crack effect during resurrection
- ✅ Success glow when completed
- ✅ Fog overlay drifting
- ✅ Floating ghost particles
- ✅ Ghost hand on drag-and-drop
- ✅ Upload progress animation

### Components
- ✅ Tombstone Card with status badges
- ✅ File Upload with drag-and-drop
- ✅ Graveyard Dashboard with filters
- ✅ Code Viewer with split panes
- ✅ Death Certificate modal
- ✅ Autopsy Report with tabs

### Theme
- ✅ Halloween color palette (dark + phosphor green)
- ✅ Gothic fonts (Creepster)
- ✅ Spooky effects (fog, particles)
- ✅ Responsive design
- ✅ Accessibility (prefers-reduced-motion)

## Mock Data

The app currently uses mock data to demonstrate all features:
- 3 sample projects (VB6, ActionScript, Silverlight)
- Different statuses (Dead, Resurrecting, Alive)
- Sample code conversion (VB6 → C#)
- Migration notes and warnings

## Next Steps

To connect to the backend:
1. Update `ProjectService` API URL
2. Implement actual file upload
3. Connect to OpenAI conversion endpoint
4. Add real-time status updates
5. Implement download functionality

## Troubleshooting

If you see errors:
- Make sure Node.js 18+ is installed
- Delete `node_modules` and run `npm install` again
- Check that port 4200 is available
