# Debug OpenAI Integration

## Issue: OpenAI API Not Being Called

### Steps to Debug

1. **Check Backend Logs**
   - Restart the backend: `dotnet run` in the `backend` folder
   - Look for the startup message:
     - ✅ `OpenAI API key is configured (length: XX)` - Good!
     - ⚠️ `OpenAI API key is not configured!` - Need to add API key

2. **Add OpenAI API Key**
   - Open `backend/appsettings.json`
   - Add your key:
     ```json
     "OpenAI": {
       "ApiKey": "sk-your-actual-openai-key-here"
     }
     ```
   - Get a key from: https://platform.openai.com/api-keys

3. **Test the Flow**
   - Upload a file (e.g., `samples/vb6-calculator.vb`)
   - Click on the tombstone card
   - Watch the backend console for these logs:
     ```
     🔄 Starting conversion for project {id}
     📄 Read {X} characters from file
     🎯 Converting VB6 to Unknown
     🤖 Calling OpenAI API...
     📡 Sending request to OpenAI API...
     ✅ Received response from OpenAI API
     ✅ OpenAI conversion completed
     ✅ Conversion completed successfully
     ```

4. **Check Frontend Console**
   - Open browser DevTools (F12)
   - Look for:
     ```
     Resurrection started: {id}
     Status update (poll 1): {status: "Processing", ...}
     Status update (poll 2): {status: "Processing", ...}
     ...
     Status update (poll N): {status: "Completed", ...}
     ✅ Resurrection completed!
     ```

5. **Common Issues**

   **Issue**: Polling never stops
   - **Cause**: Backend conversion is failing silently
   - **Fix**: Check backend logs for errors

   **Issue**: "OpenAI API key is not configured"
   - **Cause**: Missing or empty API key
   - **Fix**: Add key to `appsettings.json` and restart backend

   **Issue**: "401 Unauthorized" from OpenAI
   - **Cause**: Invalid API key
   - **Fix**: Verify your key at https://platform.openai.com/api-keys

   **Issue**: "429 Too Many Requests"
   - **Cause**: Rate limit exceeded or no credits
   - **Fix**: Check your OpenAI usage and billing

## Current Improvements

✅ Added polling timeout (max 2 minutes)
✅ Better logging in backend with emojis
✅ OpenAI API key validation on startup
✅ Detailed error messages
✅ Fixed enum mismatch between frontend/backend

## Next Steps

1. Restart backend to see new logs
2. Add OpenAI API key if missing
3. Try uploading and resurrecting a file
4. Watch the logs to see where it fails
