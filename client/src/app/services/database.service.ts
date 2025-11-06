import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  DatabaseConnection,
  TestConnectionRequest,
  TestConnectionResponse,
  ExecuteQueryRequest,
  ExecuteQueryResponse,
  SchemaInfo
} from '../models/database.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DatabaseService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  testConnection(request: TestConnectionRequest): Observable<TestConnectionResponse> {
    return this.http.post<TestConnectionResponse>(
      `${this.apiUrl}/api/database/test-connection`,
      request
    );
  }

  createConnection(connection: DatabaseConnection): Observable<{ connectionId: string; message: string }> {
    return this.http.post<{ connectionId: string; message: string }>(
      `${this.apiUrl}/api/database/connections`,
      connection
    );
  }

  getConnections(): Observable<DatabaseConnection[]> {
    return this.http.get<DatabaseConnection[]>(
      `${this.apiUrl}/api/database/connections`
    );
  }

  getConnection(connectionId: string): Observable<DatabaseConnection> {
    return this.http.get<DatabaseConnection>(
      `${this.apiUrl}/api/database/connections/${connectionId}`
    );
  }

  updateConnection(connectionId: string, connection: DatabaseConnection): Observable<any> {
    return this.http.put(
      `${this.apiUrl}/api/database/connections/${connectionId}`,
      connection
    );
  }

  deleteConnection(connectionId: string): Observable<any> {
    return this.http.delete(
      `${this.apiUrl}/api/database/connections/${connectionId}`
    );
  }

  executeQuery(request: ExecuteQueryRequest): Observable<ExecuteQueryResponse> {
    return this.http.post<ExecuteQueryResponse>(
      `${this.apiUrl}/api/database/execute-query`,
      request
    );
  }

  getSchema(connectionId: string): Observable<SchemaInfo> {
    return this.http.get<SchemaInfo>(
      `${this.apiUrl}/api/database/connections/${connectionId}/schema`
    );
  }

  getDatabases(connectionId: string): Observable<string[]> {
    return this.http.get<string[]>(
      `${this.apiUrl}/api/database/connections/${connectionId}/databases`
    );
  }
}
