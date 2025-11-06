import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WebQueryRequest, WebQueryResponse } from '../models/web-query.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class WebQueryService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  query(request: WebQueryRequest): Observable<WebQueryResponse> {
    return this.http.post<WebQueryResponse>(
      `${this.apiUrl}/api/webquery/query`,
      request
    );
  }

  getHtml(url: string, selector?: string): Observable<WebQueryResponse> {
    let params = new HttpParams().set('url', url);
    if (selector) {
      params = params.set('selector', selector);
    }
    return this.http.get<WebQueryResponse>(
      `${this.apiUrl}/api/webquery/html`,
      { params }
    );
  }

  getText(url: string, selector?: string): Observable<WebQueryResponse> {
    let params = new HttpParams().set('url', url);
    if (selector) {
      params = params.set('selector', selector);
    }
    return this.http.get<WebQueryResponse>(
      `${this.apiUrl}/api/webquery/text`,
      { params }
    );
  }

  getLinks(url: string, selector?: string): Observable<WebQueryResponse> {
    let params = new HttpParams().set('url', url);
    if (selector) {
      params = params.set('selector', selector);
    }
    return this.http.get<WebQueryResponse>(
      `${this.apiUrl}/api/webquery/links`,
      { params }
    );
  }

  getImages(url: string, selector?: string): Observable<WebQueryResponse> {
    let params = new HttpParams().set('url', url);
    if (selector) {
      params = params.set('selector', selector);
    }
    return this.http.get<WebQueryResponse>(
      `${this.apiUrl}/api/webquery/images`,
      { params }
    );
  }

  getMetadata(url: string): Observable<WebQueryResponse> {
    const params = new HttpParams().set('url', url);
    return this.http.get<WebQueryResponse>(
      `${this.apiUrl}/api/webquery/metadata`,
      { params }
    );
  }

  checkHealth(): Observable<any> {
    return this.http.get(`${this.apiUrl}/api/webquery/health`);
  }
}
