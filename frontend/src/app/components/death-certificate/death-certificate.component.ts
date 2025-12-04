import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeathCertificate } from '../../models/project.model';

@Component({
  selector: 'app-death-certificate',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './death-certificate.component.html',
  styleUrls: ['./death-certificate.component.scss']
})
export class DeathCertificateComponent {
  @Input() certificate!: DeathCertificate;
  @Output() closed = new EventEmitter<void>();

  close(): void {
    this.closed.emit();
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  getModernReplacement(): string {
    switch (this.certificate.technology) {
      case 'VB6': return 'C# .NET 8';
      case 'ActionScript': return 'TypeScript/Angular';
      case 'Silverlight': return 'Angular Material';
      case 'DotNetFramework': return '.NET 8';
      default: return 'Modern Framework';
    }
  }
}
