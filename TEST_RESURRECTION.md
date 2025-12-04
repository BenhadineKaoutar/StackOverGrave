# Test Resurrection Flow

## The Fix

**Problem**: The background task was trying to use a disposed database context.

**Solution**: Created a new service scope for the background task with its own DbContext instance.

## How to Test

1. **Stop the backend** (if running)

2. **Restart the backend** with the fix:
   ```bash
   cd backend
   dotnet run
   ```

3. **Look for startup message**:
   ```
   ✅ OpenAI API key is configured (length: 164)
   ```

4. **In the browser**:
   - Upload a file (e.g., `samples/flash-game.as`)
   - Click on the tombstone card
   - Watch it change to "Resurrecting..." with the crack animation

5. **Watch backend console** for these logs:
   ```
   info: Resurrection started: {guid}
   info: 🔄 Starting conversion for project {guid}
   info: 📄 Read XXX characters from file
   info: 🎯 Converting ActionScript to Unknown
   info: 🤖 Calling OpenAI API...
   info: 📡 Sending request to OpenAI API...
   info: ✅ Received response from OpenAI API
   info: ✅ OpenAI conversion completed
   info: ✅ Conversion completed successfully: {guid}
   ```

6. **Watch browser console** for:
   ```
   🔄 Starting resurrection for project: {guid}
   ✅ Resurrection API call successful: {guid}
   Status update (poll 1): {status: "Processing", ...}
   Status update (poll 2): {status: "Processing", ...}
   ...
   Status update (poll N): {status: "Completed", ...}
   ✅ Resurrection completed!
   ```

7. **The tombstone should change** from "Resurrecting..." to "Resurrected" with a green glow

8. **Click the completed tombstone** to view the converted code

## Expected Timeline

- Upload: < 1 second
- Analyze: < 1 second  
- Resurrect (OpenAI): 5-30 seconds depending on code size
- Total: ~10-35 seconds

## If It Still Fails

Check backend logs for specific error messages. Common issues:

1. **File not found**: Check that uploads folder exists
2. **OpenAI API error**: Check API key and credits
3. **Database error**: Delete `stackovergrave.db` and restart
