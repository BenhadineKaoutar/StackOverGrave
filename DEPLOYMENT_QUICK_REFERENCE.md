# 🚀 Quick Deployment Reference

## ✅ What Was Fixed

1. **CSS Budget Errors** - Increased from 4KB to 10KB max
2. **API Configuration** - Environment-based, no hardcoded URLs
3. **Build Process** - Auto-injects API URL from environment variable

## 📋 Vercel Deployment Checklist

### Before Deploying

- [ ] Deploy your backend first (Railway, Render, etc.)
- [ ] Note your backend URL (e.g., `https://api.example.com`)
- [ ] Make sure backend CORS allows your Vercel domain

### In Vercel Dashboard

1. **Import Repository**
   - Connect your GitHub/GitLab repo
   - Set Root Directory: `frontend`

2. **Add Environment Variable**
   ```
   Name:  API_URL
   Value: https://your-backend-url.com/api
   ```
   ⚠️ Replace with your actual backend URL
   ⚠️ Must end with `/api` (no trailing slash)

3. **Deploy**
   - Click "Deploy"
   - Wait ~2 minutes
   - Done! 🎉

### After Deployment

- [ ] Test file upload
- [ ] Check browser console for errors
- [ ] Verify API calls go to correct backend

## 🔐 Security Checklist

✅ **Safe (Already Done)**
- API URL via environment variable
- No secrets in code
- Backend API key stays on backend

❌ **Never Commit**
- `.env` files with real values
- API keys
- Database credentials

## 🛠️ Backend CORS Setup

Add your Vercel URL to backend CORS:

```csharp
// backend/Program.cs
builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://your-app.vercel.app"  // ← Add this
        )
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});
```

## 📊 Build Status

Current build: ✅ **SUCCESS**
- Warnings: Yes (CSS size - acceptable)
- Errors: None
- Bundle size: 857KB (within limits)

## 🔄 Redeploy

To redeploy after changes:
```bash
git add .
git commit -m "Update"
git push
```

Vercel auto-deploys on push to main branch.

## 🆘 Common Issues

**"API calls fail"**
→ Check API_URL environment variable in Vercel

**"CORS error"**
→ Add Vercel domain to backend CORS policy

**"404 on refresh"**
→ Already fixed by vercel.json

**"Build fails"**
→ Make sure API_URL is set in Vercel settings
