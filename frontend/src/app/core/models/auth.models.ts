export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  fullName: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  token: string;
  expiresAt: string;
  user: UserInfo;
  errorMessage?: string;
}

export interface UserInfo {
  id: string;
  email: string;
  fullName: string;
  role: UserRole;
  createdAt: string;
}

export enum UserRole {
  Admin = 'Admin',
  User = 'User',
  ReadOnly = 'ReadOnly'
}
