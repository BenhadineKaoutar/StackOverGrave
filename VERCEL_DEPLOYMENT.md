# Vercel Deployment Guide

## Issues Fixed

### 1. CSS Budget Errors ✅
Updated `angular.json` to increase component style budget from 4KB to 10KB max.

### 2. API Configuration ✅
Created environment-based configuration that keeps sensitive data out of the codebase.

## Deployment Steps

### Step 1: Deploy Backend First

Your backend needs to be deployed somewhere accessible (e.g., Railway, Render, Azure, AWS).

**Important:** Note your backend URL (e.g., `https://your-api.railway.app`)

### Step 2: Configure Vercel Environment Variables

1. Go to your Vercel project settings
2. Navigate to **Environment Variables**
3. Add the following variable:

```
Name: API_URL
Value: https://your-backend-url.com/api
```

**Note:** Do NOT include a trailing slash. The value should end with `/api`

### Step 3: Deploy Frontend to Vercel

#### Option A: Via Vercel CLI
```bash
cd frontend
npm install -g vercel
vercel
```

#### Option B: Via Vercel Dashboard
1. Go to https://vercel.com/new
2. Import your Git repository
3. Set **Root Directory** to `frontend`
4. Vercel will auto-detect Angular
5. Add the `API_URL` environment variable
6. Click **Deploy**

### Step 4: Verify Deployment

1. Visit your Vercel URL
2. Open browser DevTools → Network tab
3. Try uploading a file
4. Verify API calls go to your backend URL (not localhost)

## Environment Variables Reference

### Required
- `API_URL` - Your backend API URL (e.g., `https://api.example.com/api`)

### Optional (Backend)
- `OPENAI_API_KEY` - Set this on your backend hosting platform
- `DATABASE_URL` - If using external database

## Security Notes

✅ **Good Practices:**
- API URL is injected at build time via environment variables
- No secrets in source code
- Backend API key stays on backend only

❌ **Never Do:**
- Commit `.env` files with real values
- Put API keys in frontend code
- Hardcode production URLs

## Troubleshooting

### Build fails with "API_URL is not defined"
Make sure you set the `API_URL` environment variable in Vercel settings.

### API calls fail with CORS errors
Your backend needs to allow your Vercel domain:
```csharp
// In Program.cs
builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://your-app.vercel.app"  // Add your Vercel URL
        )
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});
```

### 404 errors on page refresh
Already handled by `vercel.json` rewrites configuration.

## Local Development

The environment setup preserves local development:

```bash
cd frontend
npm start
```

This uses `environment.development.ts` which points to `http://localhost:5017/api`

## Cost Optimization

- Vercel Free tier: 100GB bandwidth/month
- Keep bundle size under 500KB for fast loads
- Consider caching strategies for API responses
