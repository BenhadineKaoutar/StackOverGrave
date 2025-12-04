# StackOverGrave - Quick Start Guide

## 🚀 Run the App (30 seconds)

```bash
cd frontend
npm install
npm start
```

App opens at `http://localhost:4200` ✨

## 🎮 What You Can Do Right Now

### 1. View the Graveyard
- See 3 mock tombstone cards
- Watch fog drift across screen
- Notice floating ghost particles

### 2. Try Filters
- Click "VB6" to filter
- Click "All" to reset
- See counts update

### 3. Upload Files (Visual Only)
- Drag any file over upload zone
- Watch ghost hand appear
- See upload progress animation

### 4. View Code Comparison
- Click the "Alive" tombstone (MainPage.xaml)
- See VB6 → C# conversion
- Scroll through both code panes

### 5. Explore Autopsy Report
- Click "Migration Notes" tab
- Click "Dependencies" tab
- Click "Breaking Changes" tab
- Click "Warnings" tab

### 6. Navigate Back
- Click "← Back to Graveyard"
- Returns to dashboard

## 🎨 Features to Notice

### Animations
- ✅ Tombstones rise from ground
- ✅ Crack effect on "Resurrecting" card
- ✅ Glow effect on "Alive" card
- ✅ Fog drifts slowly
- ✅ Particles float upward
- ✅ Ghost hand on drag-over
- ✅ Smooth transitions everywhere

### Theme
- ✅ Dark graveyard atmosphere
- ✅ Phosphor green accents
- ✅ Gothic fonts (Creepster)
- ✅ Spooky but professional

### Responsive
- ✅ Works on mobile
- ✅ Works on tablet
- ✅ Works on desktop

## 📁 Project Structure

```
stackovergrave/
├── frontend/              ← Angular app (READY)
│   ├── src/app/
│   │   ├── components/   ← 5 components
│   │   ├── models/       ← TypeScript interfaces
│   │   └── services/     ← API service
│   └── package.json
├── backend/              ← .NET API (skeleton)
│   ├── Models/
│   ├── Services/
│   └── Data/
├── samples/              ← Test files
│   ├── vb6-calculator.vb
│   ├── flash-game.as
│   └── silverlight-form.xaml
└── .kiro/
    ├── specs/           ← 6 detailed specs
    └── steering/        ← 4 guide documents
```

## 🎯 Mock Data Included

**3 Projects:**
1. Calculator.vb (VB6) - Dead 💀
2. Game.as (ActionScript) - Resurrecting ⚡
3. MainPage.xaml (Silverlight) - Alive ✅

**1 Conversion:**
- VB6 Calculator → C# .NET 8
- 6 migration notes
- 2 dependencies
- 3 breaking changes
- 2 warnings

## 🔌 Connect to Backend (When Ready)

Update `frontend/src/app/services/project.service.ts`:

```typescript
private apiUrl = 'https://localhost:7001/api';
```

Then implement:
1. Real file upload
2. Technology detection
3. AI conversion
4. Download functionality

## 📚 Documentation

- `README.md` - Project overview
- `DEMO_GUIDE.md` - How to demo
- `FRONTEND_COMPLETE.md` - What's built
- `VISUAL_REFERENCE.md` - Design system
- `KIRO_USAGE.md` - How Kiro helped
- `START_FRONTEND.md` - Detailed setup

## 🎬 Demo in 60 Seconds

1. **Open app** (5s) - Show graveyard with fog
2. **Hover tombstone** (5s) - Show lift effect
3. **Click filter** (5s) - Show filtering
4. **Drag file** (10s) - Show ghost hand + upload
5. **Click tombstone** (5s) - Navigate to viewer
6. **Show comparison** (15s) - Scroll both panes
7. **Show autopsy** (10s) - Click through tabs
8. **Go back** (5s) - Return to graveyard

Total: 60 seconds ✨

## 🐛 Troubleshooting

**Port 4200 in use?**
```bash
npm start -- --port 4201
```

**Node modules issues?**
```bash
rm -rf node_modules package-lock.json
npm install
```

**TypeScript errors?**
```bash
npm install typescript@~5.4.2
```

## 🎨 Customization

### Change Colors
Edit `frontend/src/styles.scss`:
```scss
--accent-green: #00ff41;  // Change this!
```

### Change Fonts
Edit `frontend/src/styles.scss`:
```scss
@import url('https://fonts.googleapis.com/css2?family=YourFont');
```

### Disable Animations
Add to `styles.scss`:
```scss
* {
  animation: none !important;
  transition: none !important;
}
```

## 🚀 Next Steps

### For Demo
1. ✅ Run `npm start`
2. ✅ Practice demo flow
3. ✅ Record screen
4. ✅ Add to portfolio

### For Production
1. ⏳ Complete backend API
2. ⏳ Connect OpenAI via MCP
3. ⏳ Add authentication
4. ⏳ Deploy to Azure/Vercel

## 💡 Pro Tips

- **Animations**: Respect `prefers-reduced-motion`
- **Performance**: Lazy load code viewer route
- **Accessibility**: All interactive elements have focus states
- **Mobile**: Fully responsive, test on real devices
- **Browser**: Works in Chrome, Firefox, Safari, Edge

## 🎉 You're Ready!

The frontend is complete and demo-ready. All animations work, the theme is consistent, and the UX is smooth. Just run `npm start` and enjoy! 🪦⚡✨
