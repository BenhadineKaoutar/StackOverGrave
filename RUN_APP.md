# 🚀 Run StackOverGrave

## ⚡ Quick Start (2 Minutes)

### 1. Set OpenAI API Key

**Windows:**
```cmd
set OPENAI_API_KEY=sk-your-actual-key-here
```

**Linux/Mac:**
```bash
export OPENAI_API_KEY=sk-your-actual-key-here
```

### 2. Start Backend (Terminal 1)

```bash
cd backend
dotnet build
dotnet run
```

✅ Backend running at `https://localhost:7001`

**Note:** First time may take 30-60 seconds to build.

### 3. Start Frontend (Terminal 2)

```bash
cd frontend
npm install
npm start
```

✅ Frontend opens at `http://localhost:4200`

## 🎮 Try It Out

1. **Upload** - Drag `samples/vb6-calculator.vb` to upload zone
2. **Watch** - See tombstone rise with animation
3. **Click** - Click tombstone to view conversion
4. **Download** - Download converted project as ZIP

## 🔍 Verify Everything Works

### Check Backend
Open `https://localhost:7001/swagger`
- Should see API documentation
- Try "GET /api/graveyard" endpoint

### Check Frontend
Open `http://localhost:4200`
- Should see graveyard with fog
- Should see 3 mock tombstones
- Animations should be smooth

### Check Integration
1. Upload a file from `samples/` folder
2. Wait for analysis
3. Click "Resurrect" (requires OpenAI key)
4. Watch status change to "Resurrecting"
5. Wait for completion (~30 seconds)
6. Click tombstone to view result
7. Download converted project

## 🐛 Troubleshooting

### Backend Won't Start

**Port in use:**
```bash
dotnet run --urls "https://localhost:7002"
```

**Database error:**
```bash
rm stackovergrave.db
dotnet run
```

**OpenAI key not set:**
```bash
# Check if set
echo %OPENAI_API_KEY%  # Windows
echo $OPENAI_API_KEY   # Linux/Mac

# Set it again
set OPENAI_API_KEY=sk-xxx  # Windows
export OPENAI_API_KEY=sk-xxx  # Linux/Mac
```

### Frontend Won't Start

**Port in use:**
```bash
npm start -- --port 4201
```

**Dependencies missing:**
```bash
rm -rf node_modules package-lock.json
npm install
```

**TypeScript errors:**
```bash
npm install typescript@~5.4.2
```

### CORS Errors

Make sure:
- Backend is running at `https://localhost:7001`
- Frontend is running at `http://localhost:4200`
- Both are running simultaneously

### OpenAI Errors

**401 Unauthorized:**
- API key is invalid or not set
- Get key from https://platform.openai.com/api-keys

**429 Rate Limit:**
- Too many requests
- Wait a minute and try again
- Check your OpenAI usage limits

**500 Server Error:**
- Check backend logs
- Verify OpenAI API is working
- Try with a smaller file

## 📊 What to Expect

### First Upload (No OpenAI Key)
- ✅ File uploads successfully
- ✅ Technology detected
- ✅ Death certificate shown
- ❌ Conversion fails (needs API key)

### With OpenAI Key
- ✅ File uploads
- ✅ Technology detected
- ✅ Conversion starts
- ✅ Status updates to "Resurrecting"
- ✅ Conversion completes (~30s)
- ✅ Status updates to "Alive"
- ✅ Can view converted code
- ✅ Can download ZIP

## 🎯 Demo Checklist

Before demoing:
- [ ] Backend running
- [ ] Frontend running
- [ ] OpenAI key set
- [ ] Sample files ready
- [ ] Browser DevTools closed
- [ ] Screen recording ready

During demo:
- [ ] Show graveyard with animations
- [ ] Upload file with drag-and-drop
- [ ] Show death certificate
- [ ] Start resurrection
- [ ] Show cracking animation
- [ ] View converted code
- [ ] Show autopsy report
- [ ] Download ZIP

## 💡 Pro Tips

### Speed Up Development

**Backend hot reload:**
```bash
dotnet watch run
```

**Frontend with specific port:**
```bash
npm start -- --port 4200 --open
```

### View Logs

**Backend logs:**
```bash
dotnet run | grep -E "uploaded|analyzed|completed"
```

**Frontend logs:**
- Open browser DevTools (F12)
- Check Console tab
- Check Network tab

### Database Inspection

```bash
sqlite3 backend/stackovergrave.db
.tables
SELECT * FROM Projects;
SELECT * FROM ConversionResults;
.quit
```

### Clear Everything

```bash
# Backend
rm backend/stackovergrave.db
rm -rf backend/uploads

# Frontend
rm -rf frontend/node_modules
rm frontend/package-lock.json

# Start fresh
cd backend && dotnet run
cd frontend && npm install && npm start
```

## 🎨 Customize

### Change Colors

Edit `frontend/src/styles.scss`:
```scss
--accent-green: #00ff41;  // Change to your color
```

### Change API URL

Edit `frontend/src/app/services/project.service.ts`:
```typescript
private apiUrl = 'https://your-api.com/api';
```

### Disable Animations

Edit `frontend/src/styles.scss`:
```scss
* {
  animation: none !important;
  transition: none !important;
}
```

## 📱 Test on Mobile

1. Find your local IP:
```bash
# Windows
ipconfig

# Linux/Mac
ifconfig
```

2. Update CORS in `backend/Program.cs`:
```csharp
policy.WithOrigins(
    "http://localhost:4200",
    "http://192.168.1.x:4200"  // Your IP
)
```

3. Open on mobile:
```
http://192.168.1.x:4200
```

## 🎉 You're Ready!

Everything is set up and ready to go:
- ✅ Backend API running
- ✅ Frontend app running
- ✅ OpenAI integration ready
- ✅ Sample files included
- ✅ Documentation complete

Just run the commands and start resurrecting legacy code! 🪦⚡✨

---

**Need help?** Check:
- `BACKEND_SETUP.md` - Backend details
- `FULL_STACK_INTEGRATION.md` - Integration guide
- `PROJECT_COMPLETE.md` - Complete overview
