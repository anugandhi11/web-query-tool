export interface CreateConnectionRequest {
  name: string;
  type: DatabaseType;
  host: string;
  port: number;
  database: string;
  username: string;
  password: string;
  queryTimeoutSeconds: number;
}

export interface ConnectionResponse {
  id: string;
  name: string;
  type: DatabaseType;
  host: string;
  port: number;
  database: string;
  username: string;
  isActive: boolean;
  createdAt: string;
  lastTestedAt?: string;
  lastTestSuccessful: boolean;
  queryTimeoutSeconds: number;
}

export interface ConnectionTestResult {
  success: boolean;
  message: string;
  responseTimeMs: number;
  serverVersion?: string;
}

export enum DatabaseType {
  PostgreSQL = 'PostgreSQL',
  MySQL = 'MySQL',
  SQLServer = 'SQLServer',
  Redshift = 'Redshift'
}
