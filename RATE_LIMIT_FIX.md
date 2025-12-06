# Rate Limit Fix (429 Error)

## Problem
When uploading files for resurrection, the frontend showed a "429 Too Many Requests" error even though the backend successfully completed the conversion. The backend logs showed:
```
✅ Conversion completed successfully: 6bf4bb88-4a5e-46d2-a847-4333ccf22a74
```

## Root Cause
The status polling mechanism was calling the `/api/status/{id}` endpoint too frequently (every 2 seconds), which could trigger rate limiting from the server or intermediate proxies.

## Solution

### 1. Exponential Backoff
Changed from fixed interval polling to exponential backoff:
- Starts at 2 seconds
- Doubles delay on 429 errors (up to 10 seconds max)
- Resets to 2 seconds on successful responses

### 2. Better Error Handling
- Continues polling on 429 errors (with increased delay)
- Only stops after 5 consecutive non-429 errors
- Increased max polling time to 3 minutes (60 polls)

### 3. Silent 429 Handling
Updated error interceptor to not show toast notifications for 429 errors during status polling, since they're expected and handled gracefully.

## Changes Made

### `graveyard-dashboard.component.ts`
- Replaced `setInterval` with recursive `setTimeout` for dynamic delays
- Added exponential backoff on 429 errors
- Improved error tolerance (5 consecutive errors instead of 3)

### `error.interceptor.ts`
- Added check to skip toast notifications for 429 errors on status endpoints
- Logs warning instead of showing user-facing error

## Testing
1. Upload a file for resurrection
2. Monitor browser console for polling logs
3. If 429 occurs, you'll see: `⚠️ Rate limit hit, increasing delay to Xms`
4. Polling will continue with increased delay until completion
5. No error toast will appear for 429 during polling

## Benefits
- More resilient to rate limiting
- Better user experience (no false error messages)
- Automatic recovery from temporary rate limits
- Reduced server load with adaptive polling
