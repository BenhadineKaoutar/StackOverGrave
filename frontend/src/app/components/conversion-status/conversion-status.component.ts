import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { interval, Subscription, switchMap, takeWhile, startWith } from 'rxjs';
import { RepositoryService, RepositoryJob } from '../../services/repository.service';
import { ToastService } from '../../services/toast.service';
import { FileTreeVisualizationComponent } from '../file-tree-visualization/file-tree-visualization.component';

@Component({
  selector: 'app-conversion-status',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatProgressBarModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatCardModule,
    FileTreeVisualizationComponent
  ],
  templateUrl: './conversion-status.component.html',
  styleUrl: './conversion-status.component.scss'
})
export class ConversionStatusComponent implements OnInit, OnDestroy {
  job: RepositoryJob | null = null;
  private pollSubscription?: Subscription;
  private jobId: string = '';

  constructor(
    private route: ActivatedRoute,
    private repositoryService: RepositoryService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.jobId = this.route.snapshot.params['id'];
    this.startPolling();
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  private startPolling(): void {
    // Poll every 2 seconds
    this.pollSubscription = interval(2000).pipe(
      startWith(0), // Start immediately
      switchMap(() => this.repositoryService.getJobStatus(this.jobId)),
      takeWhile(job => {
        // Continue polling until completed or failed
        return job.status !== 'completed' && job.status !== 'failed';
      }, true) // true = include the final emission
    ).subscribe({
      next: (job) => {
        const wasCompleted = this.job?.status === 'completed';
        this.job = job;

        // Show success toast when conversion completes
        if (job.status === 'completed' && !wasCompleted) {
          this.toastService.showSuccess('🎉 Your code has been successfully resurrected! The converted files are ready for download.');
          console.log('Job completed:', job);
          console.log('Analysis Result:', job.analysisResult);
          console.log('Conversion Result:', job.conversionResult);
        }
      },
      error: (error) => {
        console.error('Error polling job status:', error);
      }
    });
  }

  private stopPolling(): void {
    if (this.pollSubscription) {
      this.pollSubscription.unsubscribe();
    }
  }

  getStatusEmoji(status: string): string {
    const emojis: { [key: string]: string } = {
      'downloading': '📥',
      'extracting': '📦',
      'analyzing': '🔍',
      'converting': '⚡',
      'packaging': '📦',
      'completed': '✅',
      'failed': '❌'
    };
    return emojis[status] || '⏳';
  }

  getStatusTitle(status: string): string {
    const titles: { [key: string]: string } = {
      'downloading': 'Downloading Repository',
      'extracting': 'Extracting Files',
      'analyzing': 'Analyzing Code',
      'converting': 'Resurrecting Code',
      'packaging': 'Packaging Results',
      'completed': 'Resurrection Complete!',
      'failed': 'Resurrection Failed'
    };
    return titles[status] || 'Processing';
  }

  getStatusClass(status: string): string {
    if (status === 'completed') return 'status-completed';
    if (status === 'failed') return 'status-failed';
    if (status === 'converting') return 'status-converting';
    return 'status-processing';
  }

  async download(): Promise<void> {
    if (this.job && this.job.status === 'completed') {
      try {
        await this.repositoryService.downloadPackage(this.jobId);
        this.toastService.showSuccess('Package downloaded successfully!');
      } catch (error) {
        // Error is already handled by the interceptor
        console.error('Download failed:', error);
      }
    }
  }

  isCompleted(): boolean {
    return this.job?.status === 'completed';
  }

  isFailed(): boolean {
    return this.job?.status === 'failed';
  }

  isProcessing(): boolean {
    return this.job?.status === 'converting';
  }

  showSpinner(): boolean {
    return this.job?.status === 'downloading' || this.job?.status === 'packaging';
  }

  getFilesAnalyzed(): number {
    if (!this.job) return 0;

    // Try different possible property names from backend
    const analysisResult = this.job.analysisResult as any;
    if (!analysisResult) return 0;

    return analysisResult.totalFiles ||
           analysisResult.TotalFiles ||
           analysisResult.total_files ||
           0;
  }

  getFilesConverted(): number {
    if (!this.job) return 0;

    // Try different possible property names from backend
    const conversionResult = this.job.conversionResult as any;
    if (!conversionResult) return 0;

    const files = conversionResult.convertedFiles ||
                  conversionResult.ConvertedFiles ||
                  conversionResult.converted_files ||
                  [];

    return Array.isArray(files) ? files.length : 0;
  }
}
