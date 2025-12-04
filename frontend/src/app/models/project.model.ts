export enum TechnologyType {
  Unknown = 'Unknown',
  VB6 = 'VB6',
  ActionScript = 'ActionScript',
  Silverlight = 'Silverlight',
  DotNetFramework = 'DotNetFramework'
}

export enum ProjectStatus {
  Uploaded = 'Uploaded',
  Processing = 'Processing',
  Completed = 'Completed',
  Failed = 'Failed'
}

// Aliases for better UX naming
export const ProjectStatusDisplay = {
  Uploaded: 'Dead',
  Processing: 'Resurrecting',
  Completed: 'Alive',
  Failed: 'Failed'
} as const;

export interface Project {
  id: string;
  originalFilename: string;
  technology: TechnologyType;
  uploadedAt: Date;
  status: ProjectStatus;
  fileSize: number;
  linesOfCode?: number;
}

export interface DeathCertificate {
  technology: TechnologyType;
  originalFilename: string;
  detectedVersion?: string;
  deprecatedDate: Date;
  causeOfDeath: string;
  fileStats: {
    linesOfCode: number;
    fileSize: number;
    complexity: string;
  };
  warnings: string[];
}
