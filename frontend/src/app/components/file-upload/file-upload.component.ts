import { Component, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent {
  @Output() fileSelected = new EventEmitter<File>();

  isDragging = signal(false);
  uploadProgress = signal(0);
  isUploading = signal(false);
  isAnalyzing = signal(false);
  currentPhase = signal<'idle' | 'uploading' | 'analyzing'>('idle');

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
      alert('File size exceeds 5MB limit');
      return;
    }

    const validExtensions = ['.vb', '.as', '.xaml', '.cs', '.vbproj', '.csproj', '.frm', '.bas', '.cls'];
    const fileExt = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();

    if (!validExtensions.includes(fileExt)) {
      alert('Unsupported file type. Please upload: ' + validExtensions.join(', '));
      return;
    }

    this.simulateUpload();
    this.fileSelected.emit(file);
  }

  private simulateUpload(): void {
    this.currentPhase.set('uploading');
    this.isUploading.set(true);
    this.uploadProgress.set(0);

    const interval = setInterval(() => {
      const current = this.uploadProgress();
      if (current >= 100) {
        clearInterval(interval);
        setTimeout(() => {
          this.isUploading.set(false);
          this.uploadProgress.set(0);
        }, 500);
      } else {
        this.uploadProgress.set(current + 10);
      }
    }, 100);
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
}
