# Backend Deployment Options

## Recommended Platforms

### 1. Railway.app ⭐ (Recommended)
**Best for:** Quick deployment, PostgreSQL support

**Pros:**
- Free tier: $5 credit/month
- Auto-deploys from GitHub
- Built-in PostgreSQL
- Easy environment variables

**Steps:**
1. Go to railway.app
2. "New Project" → "Deploy from GitHub"
3. Select your repo
4. Add environment variables:
   - `OPENAI_API_KEY=your-key`
5. Railway provides URL automatically

**Cost:** ~$5-10/month after free tier

---

### 2. Render.com
**Best for:** Free tier, simple setup

**Pros:**
- Free tier available
- Auto-deploys from GitHub
- PostgreSQL included
- SSL certificates

**Steps:**
1. Go to render.com
2. "New" → "Web Service"
3. Connect GitHub repo
4. Build Command: `dotnet publish -c Release`
5. Start Command: `dotnet backend/bin/Release/net8.0/publish/StackOverGrave.Api.dll`
6. Add environment variables

**Cost:** Free tier available, $7/month for paid

---

### 3. Azure App Service
**Best for:** Enterprise, Microsoft stack

**Pros:**
- Native .NET support
- Scalable
- Azure ecosystem integration

**Steps:**
1. Install Azure CLI
2. `az login`
3. `az webapp up --name your-app --runtime "DOTNETCORE:8.0"`
4. Set environment variables in portal

**Cost:** ~$13/month (B1 tier)

---

### 4. AWS Elastic Beanstalk
**Best for:** AWS ecosystem

**Pros:**
- Auto-scaling
- Load balancing
- AWS integration

**Steps:**
1. Install EB CLI
2. `eb init`
3. `eb create`
4. `eb setenv OPENAI_API_KEY=your-key`

**Cost:** ~$15-30/month

---

## Environment Variables Needed

All platforms need these:

```bash
OPENAI_API_KEY=sk-...your-key...
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5017
```

Optional:
```bash
DATABASE_URL=postgresql://...  # If using external DB
CORS_ORIGINS=https://your-app.vercel.app
```

---

## Quick Comparison

| Platform | Free Tier | Ease | .NET Support | Cost/Month |
|----------|-----------|------|--------------|------------|
| Railway  | $5 credit | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | $5-10 |
| Render   | Yes       | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | Free-$7 |
| Azure    | No        | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | $13+ |
| AWS      | Limited   | ⭐⭐ | ⭐⭐⭐⭐ | $15+ |

---

## After Backend Deployment

1. **Get your backend URL**
   - Example: `https://stackovergrave-api.railway.app`

2. **Update Vercel environment variable**
   ```
   API_URL=https://stackovergrave-api.railway.app/api
   ```

3. **Update backend CORS**
   ```csharp
   policy.WithOrigins(
       "https://your-app.vercel.app"
   )
   ```

4. **Test the connection**
   - Open your Vercel app
   - Try uploading a file
   - Check browser console

---

## Database Considerations

Your app uses SQLite (`stackovergrave.db`). For production:

### Option 1: Keep SQLite (Simple)
- Works fine for low traffic
- File stored on server
- May lose data on redeployments (some platforms)

### Option 2: PostgreSQL (Recommended)
- Persistent storage
- Better for production
- Requires code changes:

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

```csharp
// Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        Environment.GetEnvironmentVariable("DATABASE_URL")
    )
);
```

---

## Cost Estimate (Monthly)

**Minimal Setup:**
- Backend (Render Free): $0
- Frontend (Vercel Free): $0
- OpenAI API: ~$5-20 (usage-based)
- **Total: $5-20/month**

**Production Setup:**
- Backend (Railway): $10
- Frontend (Vercel Pro): $20
- Database (Railway): Included
- OpenAI API: ~$20-50
- **Total: $50-80/month**
