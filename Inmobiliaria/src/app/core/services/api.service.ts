import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiBaseUrl;

  get<T>(path: string) { return this.http.get<T>(`${this.baseUrl}${path}`); }
  post<T>(path: string, body: any) { return this.http.post<T>(`${this.baseUrl}${path}`, body); }
  put<T>(path: string, body: any) { return this.http.put<T>(`${this.baseUrl}${path}`, body); }
  delete<T>(path: string) { return this.http.delete<T>(`${this.baseUrl}${path}`); }
}
