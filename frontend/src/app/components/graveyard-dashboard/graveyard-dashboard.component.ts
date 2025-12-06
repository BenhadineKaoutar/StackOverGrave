import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TombstoneCardComponent } from '../tombstone-card/tombstone-card.component';
import { FileUploadComponent } from '../file-upload/file-upload.component';
import { ErrorDialogComponent } from '../error-dialog/error-dialog.component';
import { ProjectService } from '../../services/project.service';
import { Project, TechnologyType, ProjectStatus } from '../../models/project.model';

@Component({
  selector: 'app-graveyard-dashboard',
  standalone: true,
  imports: [CommonModule, TombstoneCardComponent, FileUploadComponent, ErrorDialogComponent],
  templateUrl: './graveyard-dashboard.component.html',
  styleUrls: ['./graveyard-dashboard.component.scss']
})
export class GraveyardDashboardComponent implements OnInit {
  projects = signal<Project[]>([]);
  selectedFilters = signal<TechnologyType[]>([]);
  uploadProgress = signal(0);
  isUploading = signal(false);

  // Error dialog state
  showErrorDialog = signal(false);
  errorDialogTitle = signal('');
  errorDialogMessage = signal('');
  errorDialogDetails = signal<string[]>([]);

  TechnologyType = TechnologyType;

  filteredProjects = computed(() => {
    const filters = this.selectedFilters();
    const allProjects = this.projects();

    if (filters.length === 0) {
      return allProjects;
    }

    return allProjects.filter(p => filters.includes(p.technology));
  });

  constructor(
    private projectService: ProjectService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.projectService.getProjects().subscribe({
      next: (projects) => {
        // Map the projects and ensure proper types
        const mappedProjects = projects.map(p => ({
          ...p,
          technology: this.mapTechnology(p.technology),
          status: this.mapStatus(p.status),
          uploadedAt: new Date(p.uploadedAt),
          deprecatedDate: p.deprecatedDate ? new Date(p.deprecatedDate) : undefined
        }));
        this.projects.set(mappedProjects);
      },
      error: (error) => {
        console.error('Error loading projects:', error);
        // Show empty state on error
        this.projects.set([]);
      }
    });
  }

  onFileSelected(file: File): void {
    console.log('File selected:', file.name);

    this.isUploading.set(true);
    this.uploadProgress.set(0);

    // Upload file to backend with progress tracking
    this.projectService.uploadFileWithProgress(file).subscribe({
      next: (event) => {
        if (typeof event === 'number') {
          // Progress update
          this.uploadProgress.set(event);
        } else {
          // Upload complete, got response
          console.log('File uploaded:', event.id);
          this.isUploading.set(false);
          this.uploadProgress.set(0);

          // Analyze the file
          this.projectService.analyzeFile(event.id).subscribe({
            next: (certificate) => {
              console.log('File analyzed:', certificate);

              // Add new project to list
              const newProject: Project = {
                id: event.id,
                originalFilename: file.name,
                technology: this.mapTechnology(certificate.technology),
                uploadedAt: new Date(),
                status: ProjectStatus.Uploaded,
                fileSize: file.size,
                linesOfCode: certificate.fileStats.linesOfCode,
                deprecatedDate: new Date(certificate.deprecatedDate)
              };

              this.projects.update(projects => [newProject, ...projects]);

              // Optionally show death certificate
              // this.showDeathCertificate(certificate);
            },
            error: (error) => {
              console.error('Error analyzing file:', error);

              // Check if it's an unsupported technology error
              if (error.status === 400 && error.error?.supportedTechnologies) {
                this.showError(
                  'Unsupported File Type',
                  `The file "${error.error.detectedFile}" is not supported for resurrection.`,
                  error.error.supportedTechnologies
                );
              } else {
                this.showError(
                  'Analysis Failed',
                  'Failed to analyze file. Please try again.',
                  []
                );
              }
            }
          });
        }
      },
      error: (error) => {
        console.error('Error uploading file:', error);
        this.isUploading.set(false);
        this.uploadProgress.set(0);
        this.showError(
          'Upload Failed',
          'Failed to upload file. Please try again.',
          []
        );
      }
    });
  }

  private mapTechnology(tech: string): TechnologyType {
    switch (tech) {
      case 'VB6': return TechnologyType.VB6;
      case 'ActionScript': return TechnologyType.ActionScript;
      case 'Silverlight': return TechnologyType.Silverlight;
      case 'DotNetFramework': return TechnologyType.DotNetFramework;
      default: return TechnologyType.Unknown;
    }
  }

  private mapStatus(status: string): ProjectStatus {
    switch (status) {
      case 'Uploaded': return ProjectStatus.Uploaded;
      case 'Processing': return ProjectStatus.Processing;
      case 'Completed': return ProjectStatus.Completed;
      case 'Failed': return ProjectStatus.Failed;
      default: return ProjectStatus.Uploaded;
    }
  }

  onTombstoneClick(projectId: string): void {
    const project = this.projects().find(p => p.id === projectId);

    // Only navigate if project is completed
    if (project?.status === ProjectStatus.Completed) {
      this.router.navigate(['/project', projectId]);
    } else if (project?.status === ProjectStatus.Uploaded) {
      // Start resurrection
      this.startResurrection(projectId);
    }
  }

  private startResurrection(projectId: string): void {
    console.log('🔄 Starting resurrection for project:', projectId);

    this.projectService.resurrectCode(projectId).subscribe({
      next: () => {
        console.log('✅ Resurrection API call successful:', projectId);

        // Update status to Processing
        this.projects.update(projects =>
          projects.map(p =>
            p.id === projectId
              ? { ...p, status: ProjectStatus.Processing }
              : p
          )
        );

        // Start polling for status
        this.pollStatus(projectId);
      },
      error: (error) => {
        console.error('Error starting resurrection:', error);
        this.showError(
          'Resurrection Failed',
          'Failed to start resurrection. Make sure OpenAI API key is configured.',
          []
        );
      }
    });
  }

  private pollStatus(projectId: string): void {
    let pollCount = 0;
    const maxPolls = 60; // Max 3 minutes
    let consecutiveErrors = 0;
    const maxConsecutiveErrors = 5;
    let currentDelay = 2000; // Start with 2 seconds
    const maxDelay = 10000; // Max 10 seconds

    const scheduleNextPoll = () => {
      setTimeout(() => {
        pollCount++;

        if (pollCount > maxPolls) {
          console.error('Polling timeout - max attempts reached');
          this.showError(
            'Resurrection Timeout',
            'Resurrection is taking too long. Please check backend logs.',
            []
          );
          return;
        }

        this.projectService.getStatus(projectId).subscribe({
          next: (status) => {
            consecutiveErrors = 0; // Reset error counter on success
            currentDelay = 2000; // Reset delay on success
            console.log(`Status update (poll ${pollCount}):`, status);

            if (status.status === 'Completed') {
              console.log('✅ Resurrection completed!');

              // Update status to Completed
              this.projects.update(projects =>
                projects.map(p =>
                  p.id === projectId
                    ? { ...p, status: ProjectStatus.Completed }
                    : p
                )
              );
            } else if (status.status === 'Failed') {
              console.error('❌ Resurrection failed');

              // Update status to Failed
              this.projects.update(projects =>
                projects.map(p =>
                  p.id === projectId
                    ? { ...p, status: ProjectStatus.Failed }
                    : p
                )
              );

              this.showError(
                'Resurrection Failed',
                'The resurrection process failed. Please check the backend logs for details.',
                []
              );
            } else {
              // Still processing, schedule next poll
              scheduleNextPoll();
            }
          },
          error: (error) => {
            consecutiveErrors++;
            console.error(`Error polling status (${consecutiveErrors}/${maxConsecutiveErrors}):`, error);

            // If we hit rate limit (429), use exponential backoff
            if (error.status === 429) {
              currentDelay = Math.min(currentDelay * 2, maxDelay);
              console.warn(`⚠️ Rate limit hit, increasing delay to ${currentDelay}ms`);
              scheduleNextPoll();
              return;
            }

            // Only stop polling after multiple consecutive non-429 errors
            if (consecutiveErrors >= maxConsecutiveErrors) {
              console.error('❌ Too many consecutive errors, stopping polling');

              // Update status to Failed
              this.projects.update(projects =>
                projects.map(p =>
                  p.id === projectId
                    ? { ...p, status: ProjectStatus.Failed }
                    : p
                )
              );
            } else {
              // Continue polling for other errors
              scheduleNextPoll();
            }
          }
        });
      }, currentDelay);
    };

    // Start polling
    scheduleNextPoll();
  }

  toggleFilter(tech: TechnologyType): void {
    const current = this.selectedFilters();
    if (current.includes(tech)) {
      this.selectedFilters.set(current.filter(t => t !== tech));
    } else {
      this.selectedFilters.set([...current, tech]);
    }
  }

  isFilterActive(tech: TechnologyType): boolean {
    return this.selectedFilters().includes(tech);
  }

  getFilterCount(tech: TechnologyType): number {
    return this.projects().filter(p => p.technology === tech).length;
  }

  navigateToRepositoryImport(): void {
    this.router.navigate(['/repository/import']);
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
