import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import {
  ConnectionResponse,
  CreateConnectionRequest,
  ConnectionTestResult
} from '../models/connection.models';

@Injectable({
  providedIn: 'root'
})
export class ConnectionService {
  private readonly http = inject(HttpClient);
  private readonly API_URL = '/api/v1/connection';

  // Signal for reactive connection list
  connections = signal<ConnectionResponse[]>([]);
  selectedConnection = signal<ConnectionResponse | null>(null);

  getConnections(): Observable<ConnectionResponse[]> {
    return this.http.get<ConnectionResponse[]>(this.API_URL).pipe(
      tap(connections => this.connections.set(connections))
    );
  }

  getConnection(id: string): Observable<ConnectionResponse> {
    return this.http.get<ConnectionResponse>(`${this.API_URL}/${id}`).pipe(
      tap(connection => this.selectedConnection.set(connection))
    );
  }

  createConnection(request: CreateConnectionRequest): Observable<ConnectionResponse> {
    return this.http.post<ConnectionResponse>(this.API_URL, request).pipe(
      tap(connection => {
        const current = this.connections();
        this.connections.set([...current, connection]);
      })
    );
  }

  updateConnection(id: string, request: CreateConnectionRequest): Observable<ConnectionResponse> {
    return this.http.put<ConnectionResponse>(`${this.API_URL}/${id}`, request).pipe(
      tap(updated => {
        const current = this.connections();
        const index = current.findIndex(c => c.id === id);
        if (index !== -1) {
          current[index] = updated;
          this.connections.set([...current]);
        }
      })
    );
  }

  deleteConnection(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`).pipe(
      tap(() => {
        const current = this.connections();
        this.connections.set(current.filter(c => c.id !== id));
      })
    );
  }

  testConnection(id: string): Observable<ConnectionTestResult> {
    return this.http.post<ConnectionTestResult>(`${this.API_URL}/${id}/test`, {});
  }

  testNewConnection(request: CreateConnectionRequest): Observable<ConnectionTestResult> {
    return this.http.post<ConnectionTestResult>(`${this.API_URL}/test`, request);
  }
}
