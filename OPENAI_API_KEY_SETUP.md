# 🔑 OpenAI API Key Setup Guide

## ⚠️ IMPORTANT: Backend Only!

The OpenAI API key should **ONLY** be set on your backend server. **NEVER** put it in your frontend code or environment variables.

---

## 1️⃣ Local Development

### Option A: appsettings.Development.json (Recommended)

Create or edit `backend/appsettings.Development.json`:

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-actual-openai-api-key-here"
  }
}
```

This file should be in `.gitignore` (it already is).

### Option B: User Secrets (Most Secure)

```bash
cd backend
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-actual-key-here"
```

### Option C: Environment Variable

**Windows (PowerShell):**
```powershell
$env:OpenAI__ApiKey = "sk-your-actual-key-here"
dotnet run
```

**Windows (CMD):**
```cmd
set OpenAI__ApiKey=sk-your-actual-key-here
dotnet run
```

**Linux/Mac:**
```bash
export OpenAI__ApiKey="sk-your-actual-key-here"
dotnet run
```

Note: Use double underscore `__` for nested config in environment variables.

---

## 2️⃣ Production Deployment

### Railway.app

1. Go to your Railway project
2. Click on your backend service
3. Go to **Variables** tab
4. Add new variable:
   ```
   Name:  OpenAI__ApiKey
   Value: sk-your-actual-key-here
   ```
5. Deploy/Restart

### Render.com

1. Go to your Render dashboard
2. Select your backend service
3. Go to **Environment** tab
4. Add environment variable:
   ```
   Key:   OpenAI__ApiKey
   Value: sk-your-actual-key-here
   ```
5. Save (auto-redeploys)

### Azure App Service

1. Go to Azure Portal
2. Navigate to your App Service
3. Go to **Configuration** → **Application settings**
4. Click **New application setting**:
   ```
   Name:  OpenAI__ApiKey
   Value: sk-your-actual-key-here
   ```
5. Click **Save**

### AWS Elastic Beanstalk

```bash
eb setenv OpenAI__ApiKey=sk-your-actual-key-here
```

Or via AWS Console:
1. Go to Elastic Beanstalk
2. Select your environment
3. Configuration → Software
4. Add environment property

### Docker/Docker Compose

```yaml
# docker-compose.yml
services:
  backend:
    environment:
      - OpenAI__ApiKey=sk-your-actual-key-here
```

Or use `.env` file (don't commit it):
```bash
# .env
OpenAI__ApiKey=sk-your-actual-key-here
```

---

## 3️⃣ Get Your OpenAI API Key

1. Go to https://platform.openai.com/
2. Sign up or log in
3. Go to **API Keys** section
4. Click **Create new secret key**
5. Copy the key (starts with `sk-`)
6. **Save it securely** - you can't see it again!

---

## 4️⃣ Verify Configuration

### Check Backend Logs

When you start your backend, you should see:

✅ **Success:**
```
✅ OpenAI API key is configured (length: 51)
```

❌ **Missing:**
```
⚠️  OpenAI API key is not configured! Code conversion will fail.
⚠️  Please add your OpenAI API key to appsettings.json under 'OpenAI:ApiKey'
```

### Test API Call

Upload a file through your frontend and check if conversion works.

---

## 5️⃣ Security Best Practices

### ✅ DO:
- Store in environment variables on production
- Use `appsettings.Development.json` locally (gitignored)
- Use user secrets for local development
- Rotate keys periodically
- Use separate keys for dev/prod

### ❌ DON'T:
- Commit keys to Git
- Put keys in frontend code
- Share keys in chat/email
- Use production keys in development
- Hardcode keys in source files

---

## 6️⃣ Configuration Priority

.NET reads configuration in this order (last wins):

1. `appsettings.json` (default: `"*"`)
2. `appsettings.{Environment}.json`
3. User Secrets (development only)
4. Environment Variables
5. Command-line arguments

So environment variables will override `appsettings.json`.

---

## 7️⃣ Cost Management

Your current config in `appsettings.json`:
```json
{
  "OpenAI": {
    "Model": "gpt-3.5-turbo",
    "MaxCostPerConversion": 7.0
  }
}
```

**Estimated Costs:**
- Small file (< 500 lines): ~$0.01-0.05
- Medium file (500-2000 lines): ~$0.05-0.20
- Large file (2000+ lines): ~$0.20-1.00
- Repository conversion: ~$1-5

**Monthly estimates:**
- Light use (10 conversions): ~$1-5
- Medium use (50 conversions): ~$5-20
- Heavy use (200 conversions): ~$20-100

Set usage limits in OpenAI dashboard to prevent surprises.

---

## 8️⃣ Troubleshooting

### "OpenAI API key is not configured"

**Check:**
1. Environment variable name: `OpenAI__ApiKey` (double underscore)
2. Key starts with `sk-`
3. No extra spaces or quotes
4. Backend restarted after setting variable

### "401 Unauthorized" from OpenAI

**Causes:**
- Invalid API key
- Expired API key
- Key not activated
- Billing not set up on OpenAI account

**Fix:**
1. Go to OpenAI dashboard
2. Check billing settings
3. Generate new API key
4. Update environment variable

### "429 Rate Limit" from OpenAI

**Causes:**
- Too many requests
- Free tier limits exceeded
- Need to upgrade OpenAI plan

**Fix:**
1. Wait a few minutes
2. Upgrade OpenAI plan
3. Implement request queuing (already done in your app)

---

## 9️⃣ Example: Complete Local Setup

```bash
# 1. Navigate to backend
cd backend

# 2. Create development settings file
cat > appsettings.Development.json << 'EOF'
{
  "OpenAI": {
    "ApiKey": "sk-proj-your-actual-key-here"
  }
}
EOF

# 3. Verify it's gitignored
git status  # Should not show appsettings.Development.json

# 4. Run backend
dotnet run

# 5. Check logs for:
# ✅ OpenAI API key is configured (length: 51)
```

---

## 🆘 Quick Reference

| Environment | Where to Set | Format |
|-------------|--------------|--------|
| Local Dev | `appsettings.Development.json` | `"OpenAI": { "ApiKey": "sk-..." }` |
| Railway | Variables tab | `OpenAI__ApiKey=sk-...` |
| Render | Environment tab | `OpenAI__ApiKey=sk-...` |
| Azure | Application Settings | `OpenAI__ApiKey=sk-...` |
| Docker | Environment vars | `OpenAI__ApiKey=sk-...` |

**Remember:** Double underscore `__` for nested config in environment variables!
