import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { TreeNodeComponent } from '../tree-node/tree-node.component';

export interface TreeNode {
  name: string;
  path: string;
  type: 'file' | 'directory';
  children?: TreeNode[];
  lineCount?: number;
  converted?: boolean;
  skipped?: boolean;
  isExpanded?: boolean;
}

export interface FileInfo {
  relativePath: string;
  lineCount: number;
  selectedForConversion?: boolean;
}

export interface ConvertedFileInfo {
  originalPath: string;
  convertedPath: string;
}

@Component({
  selector: 'app-file-tree-visualization',
  standalone: true,
  imports: [CommonModule, MatIconModule, TreeNodeComponent],
  templateUrl: './file-tree-visualization.component.html',
  styleUrl: './file-tree-visualization.component.scss'
})
export class FileTreeVisualizationComponent implements OnChanges {
  @Input() analysisResult?: any;
  @Input() conversionResult?: any;

  originalTree: TreeNode[] = [];
  convertedTree: TreeNode[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['analysisResult'] || changes['conversionResult']) {
      this.buildTrees();
    }
  }

  private buildTrees(): void {
    if (!this.analysisResult) {
      return;
    }

    // Build original tree
    const originalFiles = this.analysisResult.prioritizedFiles || [];
    this.originalTree = this.buildTreeFromFiles(originalFiles, false);

    // Build converted tree if conversion is complete
    if (this.conversionResult && this.conversionResult.convertedFiles) {
      const convertedFiles = this.conversionResult.convertedFiles.map((cf: ConvertedFileInfo) => ({
        relativePath: cf.convertedPath,
        lineCount: 0,
        converted: true
      }));

      // Mark which files were converted vs skipped
      const convertedPaths = new Set(
        this.conversionResult.convertedFiles.map((cf: ConvertedFileInfo) => cf.originalPath)
      );

      const allFilesWithStatus = originalFiles.map((file: FileInfo) => ({
        ...file,
        converted: convertedPaths.has(file.relativePath),
        skipped: !convertedPaths.has(file.relativePath) && file.selectedForConversion
      }));

      this.convertedTree = this.buildTreeFromFiles(convertedFiles, true);
      this.originalTree = this.buildTreeFromFiles(allFilesWithStatus, false);
    }
  }

  private buildTreeFromFiles(files: any[], isConverted: boolean): TreeNode[] {
    const root: { [key: string]: TreeNode } = {};

    files.forEach((file: any) => {
      const path = file.relativePath || file.path || '';
      const parts = path.split('/').filter((p: string) => p.length > 0);

      let currentLevel = root;
      let currentPath = '';

      parts.forEach((part: string, index: number) => {
        currentPath = currentPath ? `${currentPath}/${part}` : part;
        const isFile = index === parts.length - 1;

        if (!currentLevel[part]) {
          currentLevel[part] = {
            name: part,
            path: currentPath,
            type: isFile ? 'file' : 'directory',
            children: isFile ? undefined : {},
            lineCount: isFile ? file.lineCount : undefined,
            converted: file.converted || false,
            skipped: file.skipped || false,
            isExpanded: true
          } as any;
        }

        if (!isFile) {
          currentLevel = (currentLevel[part] as any).children;
        }
      });
    });

    return this.convertToArray(root);
  }

  private convertToArray(obj: { [key: string]: TreeNode }): TreeNode[] {
    return Object.values(obj).map(node => {
      if (node.type === 'directory' && (node as any).children) {
        node.children = this.convertToArray((node as any).children);
      }
      return node;
    }).sort((a, b) => {
      // Directories first, then files
      if (a.type !== b.type) {
        return a.type === 'directory' ? -1 : 1;
      }
      return a.name.localeCompare(b.name);
    });
  }

  toggleNode(node: TreeNode): void {
    if (node.type === 'directory') {
      node.isExpanded = !node.isExpanded;
    }
  }
}
