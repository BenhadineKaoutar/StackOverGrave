import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { DragDropDirective } from '../../directives/drag-drop.directive';
import { RepositoryService } from '../../services/repository.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-repository-import',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    DragDropDirective
  ],
  templateUrl: './repository-import.component.html',
  styleUrls: ['./repository-import.component.scss']
})
export class RepositoryImportComponent {
  gitUrl = signal('');
  isImporting = signal(false);
  isUploading = signal(false);
  uploadProgress = signal(0);
  errorMessage = signal<string | null>(null);

  constructor(
    private repositoryService: RepositoryService,
    private router: Router,
    private toastService: ToastService
  ) {}

  async importFromGit(): Promise<void> {
    const url = this.gitUrl();

    if (!url.trim()) {
      this.errorMessage.set('Please enter a repository URL');
      return;
    }

    // Validate Git URL format
    if (!this.isValidGitUrl(url)) {
      this.errorMessage.set('Invalid Git URL. Please use GitHub, GitLab, or Bitbucket URLs.');
      return;
    }

    this.isImporting.set(true);
    this.errorMessage.set(null);

    try {
      const result = await this.repositoryService.importFromGit(url).toPromise();
      if (result) {
        this.toastService.showSuccess('Repository import started successfully!');
        this.router.navigate(['/repository/status', result.repository_id]);
      }
    } catch (error: any) {
      // Error is already handled by the interceptor
      this.errorMessage.set(
        error.error?.error || 'Failed to import repository. Please check the URL and try again.'
      );
    } finally {
      this.isImporting.set(false);
    }
  }

  onFileDropped(files: FileList): void {
    if (files.length > 0) {
      this.uploadZip(files[0]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.uploadZip(input.files[0]);
    }
  }

  async uploadZip(file: File): Promise<void> {
    // Validate file type
    if (!file.name.toLowerCase().endsWith('.zip')) {
      this.errorMessage.set('Please upload a ZIP file');
      return;
    }

    // Validate file size (50MB limit)
    const maxSize = 50 * 1024 * 1024;
    if (file.size > maxSize) {
      this.errorMessage.set('File size exceeds 50MB limit');
      return;
    }

    this.isUploading.set(true);
    this.uploadProgress.set(0);
    this.errorMessage.set(null);

    this.repositoryService.uploadZipWithProgress(file).subscribe({
      next: (event) => {
        if (typeof event === 'number') {
          // Progress update
          this.uploadProgress.set(event);
        } else {
          // Upload complete
          this.isUploading.set(false);
          this.uploadProgress.set(0);
          this.toastService.showSuccess('ZIP file uploaded successfully!');
          this.router.navigate(['/repository/status', event.repository_id]);
        }
      },
      error: (error: any) => {
        this.isUploading.set(false);
        this.uploadProgress.set(0);
        this.errorMessage.set(
          error.error?.error || 'Failed to upload ZIP file. Please try again.'
        );
      }
    });
  }

  private isValidGitUrl(url: string): boolean {
    const gitUrlPattern = /^https?:\/\/(github\.com|gitlab\.com|bitbucket\.org)\/[\w-]+\/[\w.-]+/i;
    return gitUrlPattern.test(url);
  }
}
