# 🚀 Quick Local Development Setup

## Prerequisites
- .NET 8.0 SDK
- Node.js 18+
- OpenAI API Key

---

## Step 1: Get OpenAI API Key

1. Go to https://platform.openai.com/api-keys
2. Create new secret key
3. Copy it (starts with `sk-`)

---

## Step 2: Configure Backend

### Option A: Using appsettings.Development.json (Recommended)

```bash
cd backend
cp appsettings.Development.json.example appsettings.Development.json
```

Then edit `backend/appsettings.Development.json` and replace the API key:
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-actual-key-here"
  }
}
```

### Option B: Using Environment Variable

**Windows PowerShell:**
```powershell
$env:OpenAI__ApiKey = "sk-your-actual-key-here"
```

**Windows CMD:**
```cmd
set OpenAI__ApiKey=sk-your-actual-key-here
```

---

## Step 3: Start Backend

```bash
cd backend
dotnet restore
dotnet run
```

You should see:
```
✅ OpenAI API key is configured (length: 51)
🚀 Backend running on http://localhost:5017
```

---

## Step 4: Start Frontend

Open a new terminal:

```bash
cd frontend
npm install
npm start
```

Browser opens automatically at http://localhost:4200

---

## Step 5: Test It

1. Upload a VB6, ActionScript, or Silverlight file
2. Click "Analyze"
3. Click "Resurrect"
4. Watch the magic happen! 🎃

---

## Troubleshooting

### Backend won't start
- Check .NET SDK: `dotnet --version` (should be 8.0+)
- Check port 5017 is free

### Frontend won't start
- Check Node.js: `node --version` (should be 18+)
- Run `npm install` again
- Delete `node_modules` and reinstall

### "OpenAI API key not configured"
- Check the key in `appsettings.Development.json`
- Make sure it starts with `sk-`
- No extra spaces or quotes
- Restart backend after changing config

### API calls fail
- Check backend is running on port 5017
- Check browser console for errors
- Verify CORS is enabled in backend

---

## Project Structure

```
StackOverGrave/
├── backend/                    # .NET 8 API
│   ├── appsettings.json       # Base config (don't edit)
│   ├── appsettings.Development.json  # Your local config (gitignored)
│   └── Program.cs
├── frontend/                   # Angular 17
│   ├── src/
│   └── package.json
└── README.md
```

---

## Next Steps

- Read `OPENAI_API_KEY_SETUP.md` for detailed configuration
- Read `DEPLOYMENT_QUICK_REFERENCE.md` for production deployment
- Check `BACKEND_DEPLOYMENT_OPTIONS.md` for hosting options

---

## Cost Tracking

Monitor your OpenAI usage:
- Dashboard: https://platform.openai.com/usage
- Set usage limits to avoid surprises
- Typical cost: $0.01-0.20 per file conversion

---

## Development Tips

### Hot Reload
- Backend: Changes require restart
- Frontend: Auto-reloads on save

### Database
- SQLite file: `backend/stackovergrave.db`
- Reset: Delete the file and restart backend

### Logs
- Backend: Console output
- Frontend: Browser DevTools console

### API Testing
- Swagger UI: http://localhost:5017/swagger
- Test endpoints without frontend
