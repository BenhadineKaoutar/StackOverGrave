import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Project, DeathCertificate } from '../models/project.model';

interface ConversionResultResponse {
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

interface StatusResponse {
  id: string;
  status: string;
  technology: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private apiUrl = 'http://localhost:5017/api';
  projects = signal<Project[]>([]);

  constructor(private http: HttpClient) {}

  uploadFile(file: File): Observable<{ id: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ id: string }>(`${this.apiUrl}/upload`, formData);
  }

  analyzeFile(id: string): Observable<DeathCertificate> {
    return this.http.get<DeathCertificate>(`${this.apiUrl}/analyze/${id}`);
  }

  resurrectCode(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/resurrect/${id}`, {});
  }

  getProjects(): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.apiUrl}/graveyard`);
  }

  getResult(id: string): Observable<ConversionResultResponse> {
    return this.http.get<ConversionResultResponse>(`${this.apiUrl}/result/${id}`);
  }

  getStatus(id: string): Observable<StatusResponse> {
    return this.http.get<StatusResponse>(`${this.apiUrl}/status/${id}`);
  }

  downloadZip(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/download/${id}`, { responseType: 'blob' });
  }
}
