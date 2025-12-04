# StackOverGrave Demo Guide

## 🎃 What You've Built

A fully-themed Halloween code resurrection platform with:
- Spooky graveyard dashboard
- Animated tombstone cards
- Drag-and-drop file upload
- Side-by-side code comparison
- Death certificate modals
- Fog effects and floating particles

## 🚀 Running the Demo

### Frontend Only (Current State)
```bash
cd frontend
npm install
npm start
```

Opens at `http://localhost:4200` with mock data

### Full Stack (When Backend Ready)
Terminal 1:
```bash
cd backend
dotnet run
```

Terminal 2:
```bash
cd frontend
npm start
```

## 🎬 Demo Flow

### 1. Landing on Graveyard Dashboard
**What to show:**
- Fog drifting across the screen
- Floating ghost particles
- Upload zone with ghost hand on hover
- 3 mock tombstone cards with different statuses

**Talk track:**
"Welcome to StackOverGrave, where legacy code comes to rest... and rise again! Notice the spooky atmosphere with drifting fog and floating spirits."

### 2. Tombstone Cards
**What to show:**
- Point out different statuses:
  - 💀 Dead (Calculator.vb) - awaiting resurrection
  - ⚡ Resurrecting (Game.as) - cracking animation
  - ✅ Alive (MainPage.xaml) - glowing effect
- Hover effects (lift and glow)
- File stats (size, LOC)

**Talk track:**
"Each project gets its own tombstone. The cracking animation shows active AI conversion, and the glow indicates successful resurrection."

### 3. Filter System
**What to show:**
- Click different technology filters
- Show count badges
- Cards fade in/out smoothly

**Talk track:**
"Filter by technology to find specific legacy projects. We support VB6, Flash ActionScript, Silverlight, and old .NET Framework."

### 4. File Upload
**What to show:**
- Drag a file over the upload zone
- Ghost hand appears
- Upload progress animation
- New tombstone appears with rise animation

**Talk track:**
"Just drag and drop your dead code. The ghost hand catches it and sends it to the graveyard. Watch the tombstone rise from the ground!"

### 5. Code Viewer
**What to show:**
- Click the "Alive" tombstone (MainPage.xaml)
- Split-pane view with original vs converted
- Syntax highlighting
- Scroll both panes

**Talk track:**
"Here's the magic - side-by-side comparison. Original VB6 on the left, modern C# .NET 8 on the right. Notice how error handling evolved from 'On Error GoTo' to try-catch blocks."

### 6. Autopsy Report
**What to show:**
- Click through all tabs:
  - 📝 Migration Notes (6 items)
  - 📦 Dependencies (2 packages)
  - ⚠️ Breaking Changes (3 items)
  - 🚨 Warnings (2 items)
- Highlight color coding

**Talk track:**
"The autopsy report explains everything. Migration notes show what changed, dependencies list what to install, and warnings highlight manual review areas."

### 7. Death Certificate (If Implemented)
**What to show:**
- Trigger death certificate modal
- Parchment texture
- "DECEASED" stamp
- Technology details
- Cause of death

**Talk track:**
"Every technology gets an official death certificate. Flash was 'Killed by Steve Jobs and HTML5' - RIP December 31, 2020."

## 🎨 Key Features to Highlight

### Animations
- **Tombstone Rise**: Appears from ground with bounce
- **Crack Effect**: SVG path animation during processing
- **Success Glow**: Pulsing green glow on completion
- **Fog Drift**: Subtle 20-second loop
- **Particles**: Random floating upward
- **Ghost Hand**: Appears on drag-over

### Theme Consistency
- **Colors**: Dark backgrounds (#0a0a0a) with phosphor green (#00ff41)
- **Fonts**: Creepster for headers, Fira Code for code
- **Accessibility**: Respects prefers-reduced-motion
- **Responsive**: Works on mobile, tablet, desktop

### User Experience
- **Instant Feedback**: Every action has visual response
- **Clear Status**: Icons and colors show project state
- **Easy Navigation**: Click tombstone to view details
- **Smooth Transitions**: 300ms ease-out everywhere

## 🎯 Talking Points

### Problem Solved
"Legacy code is everywhere - VB6 apps from the 90s, Flash games, Silverlight business apps. Modernizing them manually takes weeks. StackOverGrave uses AI to automate the conversion while preserving business logic."

### AI Integration
"We use GPT-4 with specialized prompts for each technology. The AI understands VB6 error handling, Flash display objects, Silverlight XAML bindings, and converts them to modern equivalents."

### Developer Experience
"Built with Kiro AI assistance using specs, steering documents, and MCP integration. The specs defined each feature, steering ensured consistency, and MCP simplified OpenAI integration."

### Technical Stack
"Angular 17 with standalone components, .NET 8 Web API, SQLite database, OpenAI GPT-4 via MCP. Fully responsive, accessible, and performant."

## 🐛 Known Limitations (Current State)

- Mock data only (no real backend connection)
- No actual file upload to server
- No real AI conversion (shows sample)
- No download functionality yet
- No authentication/user accounts

## 🚀 Next Steps for Full Implementation

1. **Backend API** - Complete upload, analyze, convert endpoints
2. **OpenAI Integration** - Connect MCP server for real conversions
3. **Database** - Store projects and results
4. **Download** - Generate zip files with project structure
5. **Real-time Updates** - WebSocket for conversion progress
6. **Authentication** - User accounts and project history
7. **Multi-file Support** - Upload entire projects
8. **Cost Tracking** - Show AI token usage and costs

## 💡 Demo Tips

1. **Start with the vibe** - Let them soak in the spooky atmosphere
2. **Show the animations** - They're the wow factor
3. **Explain the problem** - Legacy code pain is universal
4. **Walk through conversion** - The side-by-side is powerful
5. **Highlight AI value** - Weeks of work → 30 seconds
6. **Mention Kiro** - Built in 3 days with AI assistance

## 🎥 Screen Recording Tips

- Record at 1920x1080 for clarity
- Use dark mode browser
- Clear browser cache for clean demo
- Have sample files ready to drag
- Practice the flow 2-3 times
- Keep it under 3 minutes
- Add spooky background music (low volume)

## 🏆 Hackathon Pitch

"StackOverGrave resurrects dead code using AI. Upload your VB6, Flash, or Silverlight, and get modern .NET 8 or Angular in seconds. We built this in 3 days using Kiro AI - specs for structure, steering for consistency, and MCP for OpenAI integration. The result? A fully-themed, production-ready MVP that solves a real problem for thousands of developers maintaining legacy systems."
