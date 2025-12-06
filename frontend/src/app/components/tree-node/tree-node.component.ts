import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { TreeNode } from '../file-tree-visualization/file-tree-visualization.component';

@Component({
  selector: 'app-tree-node',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './tree-node.component.html',
  styleUrl: './tree-node.component.scss'
})
export class TreeNodeComponent {
  @Input() node!: TreeNode;
  @Input() showStatus = false;
  @Input() level = 0;

  getIcon(): string {
    if (this.node.type === 'directory') {
      return this.node.isExpanded ? '📂' : '📁';
    }

    const ext = this.node.name.split('.').pop()?.toLowerCase();
    const icons: { [key: string]: string } = {
      'vb': '💻',
      'bas': '💻',
      'cls': '💻',
      'frm': '💻',
      'cs': '💻',
      'ts': '💻',
      'as': '💻',
      'xaml': '🌐',
      'html': '🌐',
      'css': '🎨',
      'json': '📋'
    };
    return icons[ext || ''] || '📄';
  }

  getStatusIcon(): string | null {
    if (!this.showStatus || this.node.type === 'directory') {
      return null;
    }

    if (this.node.converted) {
      return '✅';
    }

    if (this.node.skipped) {
      return '⊘';
    }

    return null;
  }

  toggleNode(): void {
    if (this.node.type === 'directory') {
      this.node.isExpanded = !this.node.isExpanded;
    }
  }

  getIndentStyle(): string {
    return `${this.level * 20}px`;
  }
}
