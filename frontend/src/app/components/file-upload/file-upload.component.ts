import { Component, Output, EventEmitter, signal, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ErrorDialogComponent } from '../error-dialog/error-dialog.component';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [CommonModule, ErrorDialogComponent],
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent {
  @Output() fileSelected = new EventEmitter<File>();
  @Input() set progress(value: number) {
    this.uploadProgress.set(value);
  }
  @Input() set uploading(value: boolean) {
    this.isUploading.set(value);
    if (value) {
      this.currentPhase.set('uploading');
    } else if (this.currentPhase() === 'uploading') {
      this.currentPhase.set('idle');
    }
  }

  isDragging = signal(false);
  uploadProgress = signal(0);
  isUploading = signal(false);
  isAnalyzing = signal(false);
  currentPhase = signal<'idle' | 'uploading' | 'analyzing'>('idle');

  // Error dialog state
  showErrorDialog = signal(false);
  errorDialogTitle = signal('');
  errorDialogMessage = signal('');
  errorDialogDetails = signal<string[]>([]);

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFile(files[0]);
    }
  }

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFile(input.files[0]);
    }
  }

  private handleFile(file: File): void {
    const maxSize = 5 * 1024 * 1024; // 5MB

    if (file.size > maxSize) {
      this.showError(
        'File Too Large',
        'File size exceeds 5MB limit. Please upload a smaller file.',
        []
      );
      return;
    }

    // Updated list of supported extensions including ASP.NET Web Forms
    const validExtensions = [
      '.vb', '.frm', '.bas', '.cls',  // VB6
      '.as',                           // ActionScript
      '.xaml',                         // Silverlight
      '.cs', '.vbproj', '.csproj',    // .NET Framework
      '.aspx', '.ascx', '.master'      // ASP.NET Web Forms markup
    ];

    const fileName = file.name.toLowerCase();
    const fileExt = fileName.substring(fileName.lastIndexOf('.'));

    // Check for compound extensions like .aspx.vb or .aspx.cs
    const isAspxCodeBehind = fileName.endsWith('.aspx.vb') || fileName.endsWith('.aspx.cs') ||
                             fileName.endsWith('.ascx.vb') || fileName.endsWith('.ascx.cs') ||
                             fileName.endsWith('.ashx.vb') || fileName.endsWith('.ashx.cs');

    if (!validExtensions.includes(fileExt) && !isAspxCodeBehind) {
      this.showError(
        'Unsupported File Type',
        'This file type is not supported for resurrection.',
        [
          'VB6: .vb, .frm, .bas, .cls',
          'Flash/ActionScript: .as',
          'Silverlight: .xaml',
          '.NET Framework: .cs, .aspx, .aspx.vb, .aspx.cs, .ascx, .master'
        ]
      );
      return;
    }

    this.fileSelected.emit(file);
  }

  startAnalyzing(): void {
    this.currentPhase.set('analyzing');
    this.isAnalyzing.set(true);
  }

  finishAnalyzing(): void {
    this.currentPhase.set('idle');
    this.isAnalyzing.set(false);
  }

  reset(): void {
    this.currentPhase.set('idle');
    this.isUploading.set(false);
    this.isAnalyzing.set(false);
    this.uploadProgress.set(0);
  }

  showError(title: string, message: string, details: string[]): void {
    this.errorDialogTitle.set(title);
    this.errorDialogMessage.set(message);
    this.errorDialogDetails.set(details);
    this.showErrorDialog.set(true);
  }

  closeErrorDialog(): void {
    this.showErrorDialog.set(false);
  }
}
