# ⚡ Quick Start Guide

## 🎯 Goal
Get StackOverGrave running locally in 5 minutes.

---

## 📋 Prerequisites

- [ ] .NET 8.0 SDK installed
- [ ] Node.js 18+ installed  
- [ ] OpenAI API key (get from https://platform.openai.com/api-keys)

---

## 🚀 Setup (3 Steps)

### 1. Configure OpenAI Key

```bash
cd backend
cp appsettings.Development.json.example appsettings.Development.json
```

Edit `backend/appsettings.Development.json`:
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-actual-key-here"
  }
}
```

### 2. Start Backend

```bash
cd backend
dotnet run
```

Wait for: `✅ OpenAI API key is configured`

### 3. Start Frontend

New terminal:
```bash
cd frontend
npm install
npm start
```

Browser opens automatically! 🎉

---

## ✅ Verify It Works

1. Upload a `.vb` or `.as` file
2. Click "Analyze"
3. Click "Resurrect"
4. Download converted code

---

## 📚 Next Steps

**For Local Development:**
- Read `setup-local-dev.md`

**For Production Deployment:**
- Read `DEPLOYMENT_QUICK_REFERENCE.md`
- Read `WHERE_TO_SET_KEYS.md`

**For Detailed Configuration:**
- Read `OPENAI_API_KEY_SETUP.md`
- Read `BACKEND_DEPLOYMENT_OPTIONS.md`

---

## 🆘 Troubleshooting

**Backend won't start:**
```bash
dotnet --version  # Should be 8.0+
```

**Frontend won't start:**
```bash
node --version  # Should be 18+
cd frontend
rm -rf node_modules
npm install
```

**"OpenAI key not configured":**
- Check `backend/appsettings.Development.json`
- Key must start with `sk-`
- Restart backend

**API calls fail:**
- Backend must be running on port 5017
- Check browser console for errors

---

## 📊 What You Built

```
┌─────────────────┐
│   Frontend      │  Angular 17 + Material
│   localhost:4200│  Halloween-themed UI
└────────┬────────┘
         │
         │ HTTP
         ▼
┌─────────────────┐
│   Backend       │  .NET 8 Web API
│   localhost:5017│  AI-powered conversion
└────────┬────────┘
         │
         │ API Key
         ▼
┌─────────────────┐
│   OpenAI API    │  GPT-4o-mini
│                 │  Code conversion
└─────────────────┘
```

---

## 💰 Cost Estimate

- Small file: ~$0.01-0.05
- Medium file: ~$0.05-0.20
- Large file: ~$0.20-1.00

Set usage limits in OpenAI dashboard!

---

## 🎃 Features

✅ VB6 → C# conversion
✅ ActionScript → TypeScript
✅ Silverlight → Angular
✅ GitHub repository import
✅ Batch file conversion
✅ Migration guides
✅ Spooky Halloween theme

---

## 🔐 Security Note

Your OpenAI key is:
- ✅ Stored locally in gitignored file
- ✅ Never sent to frontend
- ✅ Only used by backend
- ✅ Safe from exposure

---

## Ready to Deploy?

See `DEPLOYMENT_QUICK_REFERENCE.md` for:
- Vercel frontend deployment
- Railway/Render backend deployment
- Environment variable setup
- Production configuration
