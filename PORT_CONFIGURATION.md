# 🔌 Port Configuration

## ✅ Current Setup

### Backend
- **HTTP:** `http://localhost:5017`
- **API Base:** `http://localhost:5017/api`
- **Swagger:** `http://localhost:5017/swagger`

### Frontend
- **URL:** `http://localhost:4200`
- **API URL:** `http://localhost:5017/api`

## 🎯 Configuration Files

### Frontend API URL
**File:** `frontend/src/app/services/project.service.ts`
```typescript
private apiUrl = 'http://localhost:5017/api';
```

### Backend CORS
**File:** `backend/Program.cs`
```csharp
policy.SetIsOriginAllowed(origin => 
    new Uri(origin).Host == "localhost")
```
✅ Allows all localhost ports

## 🧪 Test the Connection

### 1. Check Backend is Running
Open browser: `http://localhost:5017/swagger`

You should see the Swagger UI with all API endpoints.

### 2. Test API Directly
```bash
curl http://localhost:5017/api/graveyard
```

Should return: `[]` (empty array)

### 3. Test from Frontend
1. Open `http://localhost:4200`
2. Open DevTools (F12)
3. Check Network tab
4. Should see: `GET http://localhost:5017/api/graveyard`

## 🔍 Verify Configuration

### Backend Console
When you run `dotnet run`, you should see:
```
Now listening on: http://localhost:5017
```

### Frontend Console
No CORS errors! Should see:
```javascript
// Successful API calls
GET http://localhost:5017/api/graveyard 200 OK
```

## 🚀 Quick Start

### Terminal 1 - Backend
```bash
cd backend
dotnet run
```
✅ Running on `http://localhost:5017`

### Terminal 2 - Frontend
```bash
cd frontend
npm start
```
✅ Running on `http://localhost:4200`

### Browser
```
http://localhost:4200
```
✅ Frontend loads
✅ API calls to `http://localhost:5017/api`
✅ No CORS errors

## 📊 API Endpoints

All available at `http://localhost:5017/api`:

- `POST /api/upload` - Upload file
- `GET /api/analyze/{id}` - Analyze file
- `POST /api/resurrect/{id}` - Start conversion
- `GET /api/status/{id}` - Check status
- `GET /api/result/{id}` - Get result
- `GET /api/download/{id}` - Download ZIP
- `GET /api/graveyard` - List projects

## ⚠️ If Port Changes

If your backend runs on a different port, update the frontend:

**File:** `frontend/src/app/services/project.service.ts`
```typescript
private apiUrl = 'http://localhost:YOUR_PORT/api';
```

Then refresh the browser (F5).

## 🎉 You're All Set!

- ✅ Backend: `http://localhost:5017`
- ✅ Frontend: `http://localhost:4200`
- ✅ CORS: Configured
- ✅ API: Connected

**Everything is configured correctly!** 🎃
