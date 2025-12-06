import { Injectable } from '@angular/core';
import { HttpClient, HttpEventType, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, filter } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface GitImportRequest {
  url: string;
}

export interface RepositoryImportResponse {
  repository_id: string;
}

export interface RepositoryAnalysisResult {
  totalFiles: number;
  totalLinesOfCode: number;
  detectedTechnology: string;
  projectSize: string;
  prioritizedFiles: any[];
  validationErrors: string[];
}

export interface RepositoryConversionResult {
  convertedFiles: any[];
  errors: any[];
  totalTokensUsed: number;
  estimatedCost: number;
}

export interface RepositoryJob {
  id: string;
  status: 'downloading' | 'extracting' | 'analyzing' | 'converting' | 'packaging' | 'completed' | 'failed';
  progressPercentage: number;
  statusMessage: string;
  analysisResult?: RepositoryAnalysisResult;
  conversionResult?: RepositoryConversionResult;
  errors?: string[];
}

// Backend response interface (snake_case)
interface RepositoryJobResponse {
  repository_id: string;
  status: string;
  progress: number;
  message: string;
  started_at?: string;
  completed_at?: string;
  analysis_result?: any;
  conversion_result?: any;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class RepositoryService {
  private apiUrl = `${environment.apiUrl}/repository`;

  constructor(private http: HttpClient) {}

  importFromGit(url: string): Observable<RepositoryImportResponse> {
    return this.http.post<RepositoryImportResponse>(`${this.apiUrl}/import`, { url });
  }

  uploadZip(file: File): Observable<RepositoryImportResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<RepositoryImportResponse>(`${this.apiUrl}/upload`, formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe(
      filter((event): event is HttpResponse<RepositoryImportResponse> => event.type === HttpEventType.Response),
      map(event => event.body!)
    );
  }

  uploadZipWithProgress(file: File): Observable<number | RepositoryImportResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<RepositoryImportResponse>(`${this.apiUrl}/upload`, formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe(
      map(event => {
        if (event.type === HttpEventType.UploadProgress) {
          return Math.round((100 * event.loaded) / (event.total || event.loaded));
        } else if (event.type === HttpEventType.Response) {
          return event.body!;
        }
        return 0;
      })
    );
  }

  getJobStatus(id: string): Observable<RepositoryJob> {
    return this.http.get<RepositoryJobResponse>(`${this.apiUrl}/status/${id}`).pipe(
      map(response => ({
        id: response.repository_id,
        status: response.status as any,
        progressPercentage: response.progress,
        statusMessage: response.message,
        analysisResult: response.analysis_result,
        conversionResult: response.conversion_result,
        errors: response.errors
      }))
    );
  }

  async downloadPackage(id: string): Promise<void> {
    const blob = await this.http.get(
      `${this.apiUrl}/download/${id}`,
      { responseType: 'blob' }
    ).toPromise();

    if (blob) {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `project-${id}-resurrected.zip`;
      a.click();
      window.URL.revokeObjectURL(url);
    }
  }
}
