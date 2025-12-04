# Error Handling Strategy

## User-Facing Errors

### Never Show Stack Traces
- Always catch exceptions
- Log full details server-side
- Show friendly message to user

### Error Message Format
```typescript
interface UserError {
  title: string;        // "Upload Failed"
  message: string;      // "File size exceeds 5MB limit"
  action?: string;      // "Try a smaller file"
  canRetry: boolean;
}
```

## Backend Error Handling

### Global Exception Middleware
```csharp
app.UseExceptionHandler(errorApp => {
    errorApp.Run(async context => {
        var error = context.Features.Get<IExceptionHandlerFeature>();
        _logger.LogError(error.Error, "Unhandled exception");
        
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new {
            error = "An unexpected error occurred"
        });
    });
});
```

### Specific Error Types
- `FileUploadException` → 400 Bad Request
- `TechnologyNotSupportedException` → 400 Bad Request
- `AiServiceException` → 503 Service Unavailable
- `RateLimitException` → 429 Too Many Requests

## Frontend Error Handling

### HTTP Interceptor
```typescript
intercept(req: HttpRequest<any>, next: HttpHandler) {
  return next.handle(req).pipe(
    catchError((error: HttpErrorResponse) => {
      this.toastService.showError(
        this.getErrorMessage(error)
      );
      return throwError(() => error);
    })
  );
}
```

### Toast Notifications
- Error: Red background, X icon
- Warning: Orange background, ! icon
- Success: Green background, ✓ icon
- Auto-dismiss after 5 seconds (errors stay longer)

## Retry Logic

### Exponential Backoff
```typescript
retry({
  count: 3,
  delay: (error, retryCount) => timer(Math.pow(2, retryCount) * 1000)
})
```

### When to Retry
- Network errors: Yes
- 5xx errors: Yes
- 4xx errors: No (client error)
- Timeout: Yes
