# 🔑 Where to Set API Keys - Visual Guide

## The Golden Rule

```
┌─────────────────────────────────────────────────────────┐
│                                                         │
│  ❌ NEVER put OpenAI API key in frontend                │
│  ✅ ALWAYS put OpenAI API key in backend                │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## Architecture Overview

```
┌──────────────────┐         ┌──────────────────┐
│                  │         │                  │
│    FRONTEND      │         │     BACKEND      │
│   (Vercel)       │────────▶│   (Railway)      │
│                  │         │                  │
│  ❌ No API Keys  │         │  ✅ OpenAI Key   │
│                  │         │                  │
└──────────────────┘         └────────┬─────────┘
                                      │
                                      │ API Key
                                      ▼
                             ┌─────────────────┐
                             │                 │
                             │  OpenAI API     │
                             │                 │
                             └─────────────────┘
```

---

## Local Development

### Backend (Where OpenAI Key Goes)

```
backend/
├── appsettings.json                    ← Base config (committed)
├── appsettings.Development.json        ← YOUR KEY HERE (gitignored)
└── Program.cs
```

**File: `backend/appsettings.Development.json`**
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-actual-openai-key-here"  ← PUT KEY HERE
  }
}
```

### Frontend (Where Backend URL Goes)

```
frontend/
├── src/
│   └── environments/
│       ├── environment.ts              ← Production (empty API URL)
│       └── environment.development.ts  ← Dev (localhost:5017)
└── .env.example                        ← Template only
```

**File: `frontend/src/environments/environment.development.ts`**
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5017/api'  ← Backend URL only
};
```

---

## Production Deployment

### Step 1: Deploy Backend First

Choose a platform and set environment variable:

#### Railway.app
```
Dashboard → Variables → Add Variable
┌────────────────────────────────────┐
│ Name:  OpenAI__ApiKey              │
│ Value: sk-your-actual-key-here     │
└────────────────────────────────────┘
```

#### Render.com
```
Dashboard → Environment → Add Variable
┌────────────────────────────────────┐
│ Key:   OpenAI__ApiKey              │
│ Value: sk-your-actual-key-here     │
└────────────────────────────────────┘
```

#### Azure App Service
```
Portal → Configuration → Application Settings
┌────────────────────────────────────┐
│ Name:  OpenAI__ApiKey              │
│ Value: sk-your-actual-key-here     │
└────────────────────────────────────┘
```

### Step 2: Deploy Frontend

Set backend URL in Vercel:

```
Vercel Dashboard → Environment Variables
┌────────────────────────────────────────────────┐
│ Name:  API_URL                                 │
│ Value: https://your-backend.railway.app/api    │
└────────────────────────────────────────────────┘
```

---

## What Goes Where - Summary Table

| Key/Config | Frontend | Backend | Why |
|------------|----------|---------|-----|
| OpenAI API Key | ❌ NEVER | ✅ YES | Security - must stay secret |
| Backend URL | ✅ YES | ❌ NO | Frontend needs to know where to call |
| Database URL | ❌ NO | ✅ YES | Backend only |
| CORS Origins | ❌ NO | ✅ YES | Backend security config |

---

## Security Checklist

### ✅ Safe Configuration

```
✓ OpenAI key in backend environment variable
✓ Backend URL in frontend environment variable
✓ appsettings.Development.json in .gitignore
✓ No keys committed to Git
✓ Different keys for dev/prod
```

### ❌ Dangerous Configuration

```
✗ OpenAI key in frontend code
✗ OpenAI key in Git repository
✗ API keys in public files
✗ Same key for dev and prod
✗ Keys in error messages or logs
```

---

## Quick Commands Reference

### Check if OpenAI key is set (Backend)

**Local:**
```bash
cd backend
dotnet run
# Look for: ✅ OpenAI API key is configured (length: 51)
```

**Production (Railway):**
```bash
railway variables
# Should show: OpenAI__ApiKey = sk-...
```

### Check if API URL is set (Frontend)

**Local:**
```bash
cd frontend
cat src/environments/environment.development.ts
# Should show: apiUrl: 'http://localhost:5017/api'
```

**Production (Vercel):**
```bash
vercel env ls
# Should show: API_URL = https://...
```

---

## Environment Variable Naming

### Backend (.NET)

Use double underscore `__` for nested config:

```bash
# JSON structure:
{
  "OpenAI": {
    "ApiKey": "value"
  }
}

# Environment variable:
OpenAI__ApiKey=value
```

### Frontend (Angular)

Use single underscore or any name:

```bash
# Vercel:
API_URL=https://backend.com/api

# Used in code:
environment.apiUrl
```

---

## Testing Your Setup

### 1. Test Backend Locally

```bash
cd backend
dotnet run
```

Expected output:
```
✅ OpenAI API key is configured (length: 51)
🚀 Backend running on http://localhost:5017
```

### 2. Test Frontend Locally

```bash
cd frontend
npm start
```

Browser opens → Upload file → Should work!

### 3. Test Production

1. Visit your Vercel URL
2. Open DevTools → Network tab
3. Upload a file
4. Check API calls go to your backend URL
5. Verify conversion works

---

## Common Mistakes

### ❌ Mistake 1: Key in Frontend
```typescript
// DON'T DO THIS!
const OPENAI_KEY = 'sk-...';  // ❌ EXPOSED TO USERS
```

### ❌ Mistake 2: Wrong Environment Variable Name
```bash
# Wrong (single underscore):
OpenAI_ApiKey=sk-...  # ❌ Won't work

# Correct (double underscore):
OpenAI__ApiKey=sk-...  # ✅ Works
```

### ❌ Mistake 3: Committed to Git
```bash
# Check before committing:
git status
# Should NOT show:
# - appsettings.Development.json
# - .env
```

---

## Need Help?

1. **Local setup:** Read `setup-local-dev.md`
2. **Detailed config:** Read `OPENAI_API_KEY_SETUP.md`
3. **Deployment:** Read `DEPLOYMENT_QUICK_REFERENCE.md`
4. **Backend hosting:** Read `BACKEND_DEPLOYMENT_OPTIONS.md`
