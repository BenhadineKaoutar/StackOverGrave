import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TombstoneCardComponent } from '../tombstone-card/tombstone-card.component';
import { FileUploadComponent } from '../file-upload/file-upload.component';
import { ProjectService } from '../../services/project.service';
import { Project, TechnologyType, ProjectStatus } from '../../models/project.model';

@Component({
  selector: 'app-graveyard-dashboard',
  standalone: true,
  imports: [CommonModule, TombstoneCardComponent, FileUploadComponent],
  templateUrl: './graveyard-dashboard.component.html',
  styleUrls: ['./graveyard-dashboard.component.scss']
})
export class GraveyardDashboardComponent implements OnInit {
  projects = signal<Project[]>([]);
  selectedFilters = signal<TechnologyType[]>([]);

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
        this.projects.set(projects);
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

    // Upload file to backend
    this.projectService.uploadFile(file).subscribe({
      next: (response) => {
        console.log('File uploaded:', response.id);

        // Analyze the file
        this.projectService.analyzeFile(response.id).subscribe({
          next: (certificate) => {
            console.log('File analyzed:', certificate);

            // Add new project to list
            const newProject: Project = {
              id: response.id,
              originalFilename: file.name,
              technology: this.mapTechnology(certificate.technology),
              uploadedAt: new Date(),
              status: ProjectStatus.Uploaded,
              fileSize: file.size,
              linesOfCode: certificate.fileStats.linesOfCode
            };

            this.projects.update(projects => [newProject, ...projects]);

            // Optionally show death certificate
            // this.showDeathCertificate(certificate);
          },
          error: (error) => {
            console.error('Error analyzing file:', error);
            alert('Failed to analyze file. Please try again.');
          }
        });
      },
      error: (error) => {
        console.error('Error uploading file:', error);
        alert('Failed to upload file. Please try again.');
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
        alert('Failed to start resurrection. Make sure OpenAI API key is configured.');
      }
    });
  }

  private pollStatus(projectId: string): void {
    let pollCount = 0;
    const maxPolls = 60; // Max 2 minutes (60 * 2 seconds)

    const pollInterval = setInterval(() => {
      pollCount++;

      if (pollCount > maxPolls) {
        clearInterval(pollInterval);
        console.error('Polling timeout - max attempts reached');
        alert('Resurrection is taking too long. Please check backend logs.');
        return;
      }

      this.projectService.getStatus(projectId).subscribe({
        next: (status) => {
          console.log(`Status update (poll ${pollCount}):`, status);

          if (status.status === 'Completed') {
            clearInterval(pollInterval);
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
            clearInterval(pollInterval);
            console.error('❌ Resurrection failed');

            // Update status to Failed
            this.projects.update(projects =>
              projects.map(p =>
                p.id === projectId
                  ? { ...p, status: ProjectStatus.Failed }
                  : p
              )
            );

            alert('Resurrection failed. Please check the backend logs for details.');
          }
        },
        error: (error) => {
          console.error('Error polling status:', error);
          clearInterval(pollInterval);

          // Update status to Failed
          this.projects.update(projects =>
            projects.map(p =>
              p.id === projectId
                ? { ...p, status: ProjectStatus.Failed }
                : p
            )
          );
        }
      });
    }, 2000); // Poll every 2 seconds
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
}
