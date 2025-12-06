# Repository Resurrection Configuration Guide

## Overview

This document describes all configuration options for the Repository Resurrection feature.

## Configuration File: appsettings.json

### Repository Settings

```json
{
  "Repository": {
    "MaxFileSizeMB": 50,              // Max ZIP upload size
    "MaxExtractionSizeMB": 200,       // Max extracted size (zip bomb protection)
    "MaxFiles": 50,                   // Max files in repository
    "MaxLinesOfCode": 15000,          // Max total lines of code
    "MaxSingleFileLOC": 1000,         // Max lines in single file
    "SmallProjectMaxFiles": 20,       // Small project threshold (files)
    "SmallProjectMaxLOC": 5000,       // Small project threshold (LOC)
    "MediumProjectMaxFiles": 50,      // Medium project threshold (files)
    "MediumProjectMaxLOC": 15000,     // Medium project threshold (LOC)
    "MediumProjectConversionLimit": 15, // Files to convert in medium projects
    "TempDirectory": "temp",          // Temporary file storage
    "CleanupIntervalMinutes": 60,     // Cleanup interval
    "DownloadTimeoutSeconds": 60,     // Git download timeout
    "SupportedGitHosts": [            // Allowed Git platforms
      "github.com",
      "gitlab.com",
      "bitbucket.org"
    ]
  }
}
```

### OpenAI Settings

```json
{
  "OpenAI": {
    "ApiKey": "*",                    // Your OpenAI API key
    "Model": "gpt-3.5-turbo",         // Model for repository conversion
    "Temperature": 0.3,               // Lower = more deterministic
    "MaxTokensPerRequest": 2000,      // Token limit per file conversion
    "MigrationGuideMaxTokens": 3000,  // Token limit for guide generation
    "MaxCostPerConversion": 7.0       // Max cost per repository ($)
  }
}
```

### Rate Limiting Settings

```json
{
  "RateLimiting": {
    "EnableRateLimiting": true,       // Enable/disable rate limiting
    "PermitLimit": 10,                // Requests allowed per window
    "WindowSeconds": 60,              // Time window in seconds
    "QueueLimit": 5                   // Max queued requests
  }
}
```

## Environment Variables

For production deployment, you can override settings using environment variables:

### Windows
```cmd
set OPENAI_API_KEY=sk-your-key-here
set Repository__MaxFiles=50
set Repository__MaxLinesOfCode=15000
set RateLimiting__PermitLimit=10
```

### Linux/Mac
```bash
export OPENAI_API_KEY=sk-your-key-here
export Repository__MaxFiles=50
export Repository__MaxLinesOfCode=15000
export RateLimiting__PermitLimit=10
```

### Azure App Service
Configure in Application Settings:
- `OpenAI__ApiKey` = `sk-your-key-here`
- `Repository__MaxFiles` = `50`
- `Repository__MaxLinesOfCode` = `15000`
- `Repository__TempDirectory` = `/tmp/stackovergrave`
- `RateLimiting__EnableRateLimiting` = `true`

## Size Limits Explained

### Why These Limits?

| Limit | Value | Reason |
|-------|-------|--------|
| Max files | 50 | Keep conversion time under 10 minutes |
| Max LOC | 15,000 | Control AI costs (stay under $7) |
| Max single file | 1,000 | Ensure quality conversions |
| Max upload | 50MB | Prevent abuse and storage issues |
| Max extraction | 200MB | Prevent zip bomb attacks |

### Project Size Classification

**Small Projects** (≤20 files, ≤5K LOC)
- All files converted
- No migration guide
- Cost: $2-4

**Medium Projects** (21-50 files, 5K-15K LOC)
- Top 15 files converted
- Migration guide generated
- Cost: $4-7

**Large Projects** (>50 files or >15K LOC)
- Rejected with error message
- Suggestion to split repository

## Rate Limiting

### Default Configuration
- **10 requests per minute** per IP address
- **5 queued requests** maximum
- **429 Too Many Requests** response when exceeded

### Adjusting Rate Limits

For development (more permissive):
```json
{
  "RateLimiting": {
    "EnableRateLimiting": false
  }
}
```

For production (stricter):
```json
{
  "RateLimiting": {
    "EnableRateLimiting": true,
    "PermitLimit": 5,
    "WindowSeconds": 60,
    "QueueLimit": 2
  }
}
```

## Cost Controls

### Token Limits
- **2,000 tokens** per file conversion request
- **3,000 tokens** for migration guide generation
- Prevents runaway costs

### Max Cost Per Conversion
- **$7 maximum** per repository
- System logs warning if exceeded
- Helps budget OpenAI costs

### Cost Estimation
```
Small project:  20 files × $0.15 = $3.00
Medium project: 15 files × $0.30 + $1 guide = $5.50
```

## Temporary File Management

### Directory Structure
```
temp/
├── {job-id}/
│   ├── uploaded.zip
│   ├── extracted/
│   │   └── [repository files]
│   └── package/
│       └── [converted files]
```

### Cleanup Policy
- **Automatic cleanup** after job completion (success or failure)
- **Scheduled cleanup** every 60 minutes for orphaned files
- **Retention**: Final packages kept until downloaded

### Storage Requirements
- **Development**: ~1GB for temp files
- **Production**: ~10GB recommended
- **Cleanup**: Automatic, no manual intervention needed

## Git Repository Import

### Supported Platforms
- GitHub: `https://github.com/user/repo`
- GitLab: `https://gitlab.com/user/repo`
- Bitbucket: `https://bitbucket.org/user/repo`

### Branch Fallback
System tries branches in order:
1. `main`
2. `master`
3. `develop`

### Timeout
- **60 seconds** for Git download
- Prevents hanging on large repos
- Returns timeout error if exceeded

### URL Validation
- Must be from supported Git host
- Must be valid URL format
- Must be public repository (private returns 404)

## File Filtering

### Included Extensions
- `.vb` - Visual Basic 6
- `.bas` - VB6 modules
- `.cls` - VB6 classes
- `.frm` - VB6 forms
- `.as` - ActionScript
- `.xaml` - Silverlight XAML
- `.cs` - C# (for .NET Framework)

### Excluded Directories
- `bin/` - Build output
- `obj/` - Build artifacts
- `node_modules/` - NPM packages
- `.git/` - Git metadata
- `.vs/` - Visual Studio files

### Excluded Files
- `*.Designer.*` - Auto-generated
- `*.generated.*` - Auto-generated
- `*.min.*` - Minified files

## Criticality Scoring

### Score Calculation
```
Base score = 0

Entry points (Main, Program):     +100
Models/Entities directory:         +80
Services/Business directory:       +60
Controllers/Forms directory:       +40
Small files (<100 LOC):           +20
Large files (>1000 LOC):          -1000
```

### File Selection
- Files sorted by score (descending)
- Top 15 selected for medium projects
- All files selected for small projects

## Migration Guide

### Generated For
- Medium projects only (21-50 files)
- Not generated for small projects

### Contents
- Project summary
- Converted files list
- Find & replace patterns
- File-by-file notes
- Manual steps required
- Dependencies to install
- Breaking changes
- Warnings

### Token Limit
- **3,000 tokens** maximum
- Ensures guide generation stays under $0.50

## Monitoring & Logging

### What's Logged
- Repository imports (URL, size, file count)
- Analysis results (LOC, technology detected)
- Conversion progress (files completed, errors)
- Cost tracking (tokens used, estimated cost)
- Errors and warnings

### Log Levels
- **Information**: Normal operations
- **Warning**: Size limits, cost warnings
- **Error**: Conversion failures, API errors

### Example Log Output
```
[INFO] Repository import started: https://github.com/user/repo
[INFO] Analysis complete: 35 files, 8,500 LOC, VB6 detected
[INFO] Medium project: converting top 15 files
[INFO] Conversion progress: 5/15 files completed
[INFO] Migration guide generated (2,500 tokens)
[INFO] Package created: MyApp-Resurrected.zip
[INFO] Total cost: $5.25
```

## Troubleshooting

### Issue: "Repository exceeds size limits"
**Solution**: Reduce repository size or split into modules
```json
{
  "Repository": {
    "MaxFiles": 100,           // Increase if needed
    "MaxLinesOfCode": 30000    // Increase if needed
  }
}
```

### Issue: "Rate limit exceeded"
**Solution**: Adjust rate limiting or wait
```json
{
  "RateLimiting": {
    "PermitLimit": 20,         // Increase limit
    "WindowSeconds": 60
  }
}
```

### Issue: "OpenAI API error"
**Solution**: Check API key and quota
```bash
# Verify API key is set
echo $OPENAI_API_KEY

# Check OpenAI dashboard for quota
# https://platform.openai.com/usage
```

### Issue: "Temp directory full"
**Solution**: Increase cleanup frequency
```json
{
  "Repository": {
    "CleanupIntervalMinutes": 30  // More frequent cleanup
  }
}
```

## Performance Tuning

### For High Volume
```json
{
  "Repository": {
    "MaxFiles": 30,                    // Reduce for faster processing
    "MediumProjectConversionLimit": 10 // Convert fewer files
  },
  "OpenAI": {
    "MaxTokensPerRequest": 1500        // Reduce token usage
  },
  "RateLimiting": {
    "PermitLimit": 20,                 // Allow more requests
    "WindowSeconds": 60
  }
}
```

### For Cost Optimization
```json
{
  "Repository": {
    "MaxFiles": 30,                    // Smaller projects
    "MaxLinesOfCode": 10000            // Fewer lines
  },
  "OpenAI": {
    "Model": "gpt-3.5-turbo",          // Cheaper model
    "MaxTokensPerRequest": 1500,       // Fewer tokens
    "MaxCostPerConversion": 5.0        // Lower cost limit
  }
}
```

### For Quality
```json
{
  "OpenAI": {
    "Model": "gpt-4",                  // Better model
    "Temperature": 0.2,                // More deterministic
    "MaxTokensPerRequest": 3000        // More context
  }
}
```

## Security Considerations

### API Key Protection
- ✅ Store in environment variables
- ✅ Never commit to source control
- ✅ Use Azure Key Vault in production
- ❌ Don't hardcode in appsettings.json

### File Upload Security
- ✅ Size limits enforced (50MB)
- ✅ Extraction limits enforced (200MB)
- ✅ Path traversal prevention
- ✅ Malicious file detection
- ✅ Temp file cleanup

### Rate Limiting
- ✅ Prevents abuse
- ✅ Controls costs
- ✅ Per-IP limiting
- ✅ Queue management

## Production Checklist

- [ ] OpenAI API key configured
- [ ] Repository limits set appropriately
- [ ] Rate limiting enabled
- [ ] Temp directory configured with sufficient space
- [ ] Cleanup job scheduled
- [ ] Logging configured
- [ ] Monitoring alerts set up
- [ ] Cost tracking enabled
- [ ] Backup strategy defined
- [ ] Security review completed

## Support

For issues or questions:
1. Check logs for error messages
2. Review this configuration guide
3. Verify environment variables
4. Test with sample repositories
5. Check OpenAI API status

---

**Last Updated**: December 2024
**Version**: 1.0
