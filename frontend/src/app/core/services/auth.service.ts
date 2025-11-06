import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, RegisterRequest, UserInfo } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly API_URL = '/api/v1/auth';

  // Signals for reactive state
  currentUser = signal<UserInfo | null>(null);
  isAuthenticated = signal<boolean>(false);

  constructor() {
    // Check if user is already logged in
    this.loadUserFromStorage();
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.API_URL}/login`, request).pipe(
      tap(response => {
        if (response.success) {
          this.setSession(response);
        }
      })
    );
  }

  register(request: RegisterRequest): Observable<UserInfo> {
    return this.http.post<UserInfo>(`${this.API_URL}/register`, request);
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('tokenExpiry');
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isTokenExpired(): boolean {
    const expiry = localStorage.getItem('tokenExpiry');
    if (!expiry) return true;
    return Date.now() > new Date(expiry).getTime();
  }

  private setSession(response: LoginResponse): void {
    localStorage.setItem('token', response.token);
    localStorage.setItem('user', JSON.stringify(response.user));
    localStorage.setItem('tokenExpiry', response.expiresAt);
    this.currentUser.set(response.user);
    this.isAuthenticated.set(true);
  }

  private loadUserFromStorage(): void {
    const token = this.getToken();
    const userJson = localStorage.getItem('user');

    if (token && userJson && !this.isTokenExpired()) {
      const user = JSON.parse(userJson) as UserInfo;
      this.currentUser.set(user);
      this.isAuthenticated.set(true);
    }
  }
}
