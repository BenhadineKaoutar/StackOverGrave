import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastService = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Don't show toast for 429 errors on status polling (they're handled by the component)
      const isStatusPolling = req.url.includes('/status/');
      const isRateLimitError = error.status === 429;

      if (isStatusPolling && isRateLimitError) {
        // Just log it, don't show toast
        console.warn('Rate limit hit during status polling, will retry...');
        return throwError(() => error);
      }

      // Handle different error types
      if (error.error instanceof ErrorEvent) {
        // Client-side or network error
        toastService.showError('Network error occurred. Please check your connection.');
      } else {
        // Backend returned an unsuccessful response code
        const errorMessage = getErrorMessage(error);
        const details = error.error?.details;
        const suggestion = error.error?.suggestion;

        toastService.showError(errorMessage, { details, suggestion });
      }

      return throwError(() => error);
    })
  );
};

function getErrorMessage(error: HttpErrorResponse): string {
  // Check if the error response has a structured error message
  if (error.error?.error) {
    return error.error.error;
  }

  // Check for common error message fields
  if (error.error?.message) {
    return error.error.message;
  }

  // Handle specific status codes
  switch (error.status) {
    case 400:
      return 'Invalid request. Please check your input.';
    case 401:
      return 'Unauthorized. Please log in.';
    case 403:
      return 'Access forbidden.';
    case 404:
      return 'Resource not found.';
    case 429:
      return 'Too many requests. Please try again later.';
    case 500:
      return 'Server error occurred. Please try again.';
    case 503:
      return 'Service temporarily unavailable. Please try again later.';
    default:
      return error.message || 'An unexpected error occurred.';
  }
}
