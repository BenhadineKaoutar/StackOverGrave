import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ProjectService } from '../../services/project.service';

interface ConversionData {
  originalCode: string;
  convertedCode: string;
  originalFilename: string;
  convertedFilename: string;
  sourceTech: string;
  targetTech: string;
  migrationNotes: string[];
  dependencies: string[];
  breakingChanges: string[];
  warnings: string[];
}

@Component({
  selector: 'app-code-viewer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './code-viewer.component.html',
  styleUrls: ['./code-viewer.component.scss']
})
export class CodeViewerComponent implements OnInit {
  projectId = signal<string>('');
  conversionData = signal<ConversionData | null>(null);
  activeTab = signal<'notes' | 'dependencies' | 'breaking' | 'warnings'>('notes');
  isLoading = signal(true);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private projectService: ProjectService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.projectId.set(id);
      this.loadConversionData(id);
    }
  }

  loadConversionData(id: string): void {
    this.isLoading.set(true);

    // Load real data from backend
    this.projectService.getResult(id).subscribe({
      next: (result) => {
        this.conversionData.set({
          originalCode: result.originalCode,
          convertedCode: result.convertedCode,
          originalFilename: result.originalFilename,
          convertedFilename: result.convertedFilename,
          sourceTech: result.sourceTech,
          targetTech: result.targetTech,
          migrationNotes: result.migrationNotes,
          dependencies: result.dependencies,
          breakingChanges: result.breakingChanges,
          warnings: result.warnings
        });
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading conversion data:', error);
        this.isLoading.set(false);
        alert('Failed to load conversion result. The project may not be completed yet.');
        this.router.navigate(['/']);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }

  download(): void {
    const id = this.projectId();
    console.log('Download triggered for project:', id);

    this.projectService.downloadZip(id).subscribe({
      next: (blob) => {
        // Create download link
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${this.conversionData()?.originalFilename || 'converted'}_converted.zip`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error downloading:', error);
        alert('Failed to download. Please try again.');
      }
    });
  }

  setActiveTab(tab: 'notes' | 'dependencies' | 'breaking' | 'warnings'): void {
    this.activeTab.set(tab);
  }
}
