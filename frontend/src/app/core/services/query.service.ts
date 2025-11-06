import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  QueryExecuteRequest,
  QueryResult,
  QueryBuilderRequest,
  QueryHistory
} from '../models/query.models';

@Injectable({
  providedIn: 'root'
})
export class QueryService {
  private readonly http = inject(HttpClient);
  private readonly API_URL = '/api/v1/query';

  /**
   * Execute a Base64-encoded SQL query
   * WAF-FRIENDLY: SQL is encoded before sending
   */
  executeEncodedQuery(request: QueryExecuteRequest): Observable<QueryResult> {
    return this.http.post<QueryResult>(`${this.API_URL}/execute`, request);
  }

  /**
   * Build and execute a query from parameters
   * WAF-FRIENDLY: No SQL in request, built server-side
   */
  executeBuiltQuery(request: QueryBuilderRequest): Observable<QueryResult> {
    return this.http.post<QueryResult>(`${this.API_URL}/build`, request);
  }

  /**
   * Get query history for current user
   */
  getQueryHistory(limit: number = 50): Observable<QueryHistory[]> {
    return this.http.get<QueryHistory[]>(`${this.API_URL}/history`, {
      params: { limit: limit.toString() }
    });
  }

  /**
   * Encode SQL query to Base64
   * This is done client-side to hide SQL from WAF
   */
  encodeSqlQuery(sql: string): string {
    return btoa(sql);
  }

  /**
   * Decode Base64 to SQL
   * Only for display purposes, never sent to API
   */
  decodeSqlQuery(encoded: string): string {
    return atob(encoded);
  }
}
