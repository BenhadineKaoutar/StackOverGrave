# Backend Setup Guide

## 🚀 Quick Start

### 1. Set OpenAI API Key

Create `backend/appsettings.Development.json`:
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-actual-api-key-here"
  }
}
```

Or set environment variable:
```bash
# Windows
set OPENAI_API_KEY=sk-your-actual-api-key-here

# Linux/Mac
export OPENAI_API_KEY=sk-your-actual-api-key-here
```

### 2. Run the Backend

```bash
cd backend
dotnet restore
dotnet run
```

API runs at `https://localhost:7001`

### 3. Test the API

Open browser: `https://localhost:7001/swagger`

## 📡 API Endpoints

### POST /api/upload
Upload a legacy code file

**Request:**
- Content-Type: multipart/form-data
- Body: file (max 5MB)

**Response:**
```json
{
  "id": "guid-here"
}
```

### GET /api/analyze/{id}
Analyze uploaded file and get death certificate

**Response:**
```json
{
  "technology": "VB6",
  "originalFilename": "Calculator.vb",
  "deprecatedDate": "2008-04-08",
  "causeOfDeath": "Abandoned by Microsoft in favor of .NET",
  "fileStats": {
    "linesOfCode": 150,
    "fileSize": 2048,
    "complexity": "Low"
  },
  "warnings": []
}
```

### POST /api/resurrect/{id}
Start AI conversion (async)

**Response:**
```json
{
  "message": "Resurrection in progress"
}
```

### GET /api/status/{id}
Check conversion status

**Response:**
```json
{
  "id": "guid",
  "status": "Processing",
  "technology": "VB6"
}
```

Status values: `Uploaded`, `Processing`, `Completed`, `Failed`

### GET /api/result/{id}
Get conversion result

**Response:**
```json
{
  "originalCode": "VB6 code...",
  "convertedCode": "C# code...",
  "originalFilename": "Calculator.vb",
  "convertedFilename": "Calculator.cs",
  "sourceTech": "VB6",
  "targetTech": "C# .NET 8",
  "migrationNotes": ["note1", "note2"],
  "dependencies": ["package1", "package2"],
  "breakingChanges": ["change1"],
  "warnings": ["warning1"]
}
```

### GET /api/download/{id}
Download converted project as ZIP

**Response:**
- Content-Type: application/zip
- File: `{filename}_converted.zip`

### GET /api/graveyard
List all projects

**Response:**
```json
[
  {
    "id": "guid",
    "originalFilename": "Calculator.vb",
    "technology": "VB6",
    "uploadedAt": "2024-12-03T10:00:00Z",
    "status": "Completed",
    "fileSize": 2048,
    "linesOfCode": 150
  }
]
```

## 🏗️ Architecture

### Services

**ITechnologyDetectionService**
- Analyzes file to detect technology
- Generates death certificate
- Calculates file statistics

**IAiConversionService**
- Calls OpenAI API with specialized prompts
- Parses AI response
- Handles errors and retries

**IFileStorageService**
- Saves uploaded files
- Manages file system
- Cleans up old files

**IPackagingService**
- Generates ZIP files
- Creates README and migration notes
- Adds project files (.csproj, package.json)

### Database Schema

**Projects Table:**
```sql
CREATE TABLE Projects (
    Id TEXT PRIMARY KEY,
    OriginalFilename TEXT NOT NULL,
    Technology INTEGER NOT NULL,
    UploadedAt TEXT NOT NULL,
    Status INTEGER NOT NULL,
    FileSize INTEGER NOT NULL,
    LinesOfCode INTEGER,
    FilePath TEXT NOT NULL
);
```

**ConversionResults Table:**
```sql
CREATE TABLE ConversionResults (
    Id TEXT PRIMARY KEY,
    ProjectId TEXT NOT NULL,
    OriginalCode TEXT NOT NULL,
    ConvertedCode TEXT NOT NULL,
    MigrationNotes TEXT NOT NULL,
    ProcessedAt TEXT NOT NULL,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
);
```

## 🤖 OpenAI Integration

### Prompts

Each technology has a specialized prompt:

**VB6 → C# .NET 8:**
- Preserve business logic
- Use async/await
- Add dependency injection
- Replace VB6 collections with List<T>
- Use LINQ

**ActionScript → TypeScript:**
- Replace Flash display objects with Canvas/DOM
- Convert to Angular components
- Use RxJS for async
- Add TypeScript types

**Silverlight → Angular:**
- Map XAML to Angular Material
- Convert data binding
- Use reactive forms
- Convert styles to SCSS

**Old .NET → .NET 8:**
- Replace deprecated APIs
- Add nullable reference types
- Use modern C# features
- Update to .NET 8 patterns

### Response Format

AI responds with JSON:
```json
{
  "convertedCode": "// code here",
  "migrationNotes": ["note1", "note2"],
  "dependencies": ["package1"],
  "breakingChanges": ["change1"],
  "warnings": ["warning1"]
}
```

### Error Handling

- Rate limit exceeded → Retry with exponential backoff
- Invalid response → Return fallback with warnings
- Timeout → Cancel and mark as failed
- API key invalid → Return 503 error

## 📦 ZIP Package Contents

Generated ZIP includes:

```
converted-project.zip
├── src/
│   └── ConvertedCode.cs (or .ts)
├── README.md
├── MIGRATION_NOTES.md
└── Project.csproj (or package.json)
```

**README.md:**
- Original vs new technology
- Quick start instructions
- Files included
- Next steps

**MIGRATION_NOTES.md:**
- Changes made
- Dependencies required
- Breaking changes
- Warnings

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=stackovergrave.db"
  },
  "FileStorage": {
    "UploadPath": "uploads"
  },
  "OpenAI": {
    "ApiKey": "your-key-here"
  }
}
```

### Environment Variables

- `OPENAI_API_KEY` - OpenAI API key (required)
- `DATABASE_CONNECTION` - SQLite connection string
- `FILE_STORAGE_PATH` - Upload directory path

## 🧪 Testing

### Manual Testing

1. **Upload File:**
```bash
curl -X POST https://localhost:7001/api/upload \
  -F "file=@samples/vb6-calculator.vb"
```

2. **Analyze:**
```bash
curl https://localhost:7001/api/analyze/{id}
```

3. **Resurrect:**
```bash
curl -X POST https://localhost:7001/api/resurrect/{id}
```

4. **Check Status:**
```bash
curl https://localhost:7001/api/status/{id}
```

5. **Get Result:**
```bash
curl https://localhost:7001/api/result/{id}
```

6. **Download:**
```bash
curl https://localhost:7001/api/download/{id} -o converted.zip
```

### Using Swagger

1. Navigate to `https://localhost:7001/swagger`
2. Try each endpoint interactively
3. View request/response schemas

## 🐛 Troubleshooting

### Port Already in Use
```bash
dotnet run --urls "https://localhost:7002"
```

### Database Locked
```bash
rm stackovergrave.db
dotnet run
```

### OpenAI API Errors

**401 Unauthorized:**
- Check API key is set correctly
- Verify key is valid

**429 Rate Limit:**
- Wait and retry
- Upgrade OpenAI plan

**500 Server Error:**
- Check OpenAI status page
- Review logs for details

### CORS Issues

Frontend must be at `http://localhost:4200` or `http://localhost:4201`

To add more origins, edit `Program.cs`:
```csharp
policy.WithOrigins("http://localhost:4200", "http://localhost:3000")
```

## 📊 Monitoring

### Logs

Logs are written to console. Key events:
- File uploaded
- File analyzed
- Conversion started
- Conversion completed/failed
- Errors with stack traces

### Database

View projects:
```bash
sqlite3 stackovergrave.db "SELECT * FROM Projects;"
```

View results:
```bash
sqlite3 stackovergrave.db "SELECT * FROM ConversionResults;"
```

## 🚀 Production Deployment

### Azure App Service

1. Create App Service
2. Set environment variables
3. Deploy:
```bash
dotnet publish -c Release
```

### Docker

Create `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY bin/Release/net8.0/publish/ .
ENTRYPOINT ["dotnet", "StackOverGrave.Api.dll"]
```

Build and run:
```bash
docker build -t stackovergrave-api .
docker run -p 7001:80 -e OPENAI_API_KEY=sk-xxx stackovergrave-api
```

## 💰 Cost Estimation

### OpenAI Costs (GPT-4 Turbo)

- Input: $0.01 per 1K tokens
- Output: $0.03 per 1K tokens

Average conversion:
- Input: ~1000 tokens (code + prompt)
- Output: ~500 tokens (converted code)
- Cost: ~$0.025 per conversion

100 conversions/day = ~$2.50/day = ~$75/month

### Optimization

- Cache identical conversions
- Use GPT-3.5 for simple conversions
- Truncate large files
- Remove comments before sending

## 🎯 Next Steps

1. ✅ Backend API complete
2. ✅ OpenAI integration working
3. ⏳ Connect frontend to backend
4. ⏳ Add authentication
5. ⏳ Deploy to production
6. ⏳ Add monitoring/analytics

## 🎉 You're Ready!

The backend is fully functional with:
- All API endpoints working
- OpenAI integration ready
- File upload and storage
- Technology detection
- ZIP package generation
- Error handling
- CORS configured

Just add your OpenAI API key and run `dotnet run`! 🚀
