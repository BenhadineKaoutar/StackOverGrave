# ✅ Backend Fixed - Ready to Run!

## 🔧 What Was Fixed

### Issue: OpenAPI Version Conflict
The Swashbuckle package version 10.x was incompatible with .NET 9.

**Solution:** Downgraded to Swashbuckle.AspNetCore 6.5.0 (stable and compatible)

## 🚀 Run the Backend

```bash
cd backend
dotnet run
```

## 🔗 Access Points

When the backend starts, you'll see:

```
Now listening on: https://localhost:7183
Now listening on: http://localhost:5017
```

### Swagger UI
```
https://localhost:7183/swagger
```
or
```
http://localhost:5017/swagger
```

### API Base URL
```
https://localhost:7183/api
```

## 📝 Update Frontend API URL

The frontend needs to point to the correct backend URL.

Edit `frontend/src/app/services/project.service.ts`:

```typescript
private apiUrl = 'https://localhost:7183/api';  // Updated!
```

## 🧪 Test the Backend

### 1. Check Swagger
Open: `https://localhost:7183/swagger`

You should see all 7 API endpoints.

### 2. Test Graveyard Endpoint
```bash
curl https://localhost:7183/api/graveyard
```

Should return: `[]` (empty array)

### 3. Test Upload (with file)
```bash
curl -X POST https://localhost:7183/api/upload \
  -F "file=@../samples/vb6-calculator.vb"
```

Should return: `{"id":"some-guid"}`

## ⚙️ Set OpenAI API Key

Before testing AI conversion, set your API key:

**PowerShell:**
```powershell
$env:OPENAI_API_KEY = "sk-your-key-here"
```

**CMD:**
```cmd
set OPENAI_API_KEY=sk-your-key-here
```

**Or create `appsettings.Development.json`:**
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-actual-key-here"
  }
}
```

## 🎯 Complete Setup

### Terminal 1 - Backend
```bash
cd backend
$env:OPENAI_API_KEY = "sk-your-key-here"
dotnet run
```

Wait for: `Now listening on: https://localhost:7183`

### Terminal 2 - Frontend
```bash
cd frontend
npm install
npm start
```

Opens at: `http://localhost:4200`

## 🔍 Verify Everything

1. ✅ Backend running at `https://localhost:7183`
2. ✅ Swagger UI at `https://localhost:7183/swagger`
3. ✅ Frontend running at `http://localhost:4200`
4. ✅ Frontend can call backend (check browser console)

## 🐛 Troubleshooting

### Certificate Error in Browser
If you see SSL certificate warning:
- Click "Advanced"
- Click "Proceed to localhost (unsafe)"
- This is normal for local development

### Port Already in Use
```bash
# Use different port
dotnet run --urls "https://localhost:7184"
```

### CORS Error
Make sure frontend is at `http://localhost:4200` (configured in CORS policy)

## 🎉 Success!

Backend is now running with:
- ✅ Swagger UI working
- ✅ All 7 endpoints available
- ✅ Database auto-created
- ✅ CORS configured
- ✅ Ready for frontend integration

**Access Swagger:** `https://localhost:7183/swagger` 🎃
