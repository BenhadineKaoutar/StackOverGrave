import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Project, ProjectStatus, TechnologyType } from '../../models/project.model';

@Component({
  selector: 'app-tombstone-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tombstone-card.component.html',
  styleUrls: ['./tombstone-card.component.scss']
})
export class TombstoneCardComponent {
  @Input() project!: Project;
  @Output() cardClick = new EventEmitter<string>();

  ProjectStatus = ProjectStatus;
  TechnologyType = TechnologyType;

  getStatusIcon(): string {
    switch (this.project.status) {
      case ProjectStatus.Uploaded: return '💀';
      case ProjectStatus.Processing: return '⚡';
      case ProjectStatus.Completed: return '✅';
      case ProjectStatus.Failed: return '❌';
      default: return '❓';
    }
  }

  getStatusText(): string {
    switch (this.project.status) {
      case ProjectStatus.Uploaded: return 'Awaiting Resurrection';
      case ProjectStatus.Processing: return 'Resurrecting...';
      case ProjectStatus.Completed: return 'Resurrected';
      case ProjectStatus.Failed: return 'Resurrection Failed';
      default: return 'Unknown';
    }
  }

  getStatusClass(): string {
    const status = this.project.status?.toString() || 'unknown';
    return status.toLowerCase();
  }

  getTechIcon(): string {
    switch (this.project.technology) {
      case TechnologyType.VB6: return '🔷';
      case TechnologyType.ActionScript: return '⚡';
      case TechnologyType.Silverlight: return '🌙';
      case TechnologyType.DotNetFramework: return '🔵';
      default: return '❓';
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  onClick(): void {
    this.cardClick.emit(this.project.id);
  }
}
