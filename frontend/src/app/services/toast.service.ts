import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

export interface ToastOptions {
  details?: any;
  suggestion?: string;
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  constructor(private snackBar: MatSnackBar) {}

  showError(message: string, options?: ToastOptions): void {
    const duration = options?.duration || 7000; // Errors stay longer
    let displayMessage = message;

    if (options?.details) {
      displayMessage += '\n' + this.formatDetails(options.details);
    }

    if (options?.suggestion) {
      displayMessage += '\n💡 ' + options.suggestion;
    }

    this.snackBar.open(displayMessage, 'Close', {
      duration,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: ['error-toast']
    });
  }

  showWarning(message: string, duration: number = 5000): void {
    this.snackBar.open(message, 'Close', {
      duration,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: ['warning-toast']
    });
  }

  showSuccess(message: string, duration: number = 3000): void {
    this.snackBar.open(message, 'Close', {
      duration,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: ['success-toast']
    });
  }

  showInfo(message: string, duration: number = 4000): void {
    this.snackBar.open(message, 'Close', {
      duration,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: ['info-toast']
    });
  }

  private formatDetails(details: any): string {
    if (typeof details === 'string') {
      return details;
    }

    if (typeof details === 'object') {
      return Object.entries(details)
        .map(([key, value]) => `${key}: ${value}`)
        .join(', ');
    }

    return String(details);
  }
}
