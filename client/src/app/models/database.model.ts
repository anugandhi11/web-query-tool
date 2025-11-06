export enum DatabaseProvider {
  SqlServer = 'SqlServer',
  PostgreSQL = 'PostgreSQL',
  MySQL = 'MySQL'
}

export interface DatabaseConnection {
  id?: string;
  name: string;
  provider: DatabaseProvider;
  host: string;
  port: number;
  database: string;
  username: string;
  password: string;
  useSSL: boolean;
  createdAt?: Date;
}

export interface TestConnectionRequest {
  provider: DatabaseProvider;
  host: string;
  port: number;
  database: string;
  username: string;
  password: string;
  useSSL: boolean;
}

export interface TestConnectionResponse {
  success: boolean;
  message?: string;
  serverVersion?: string;
}

export interface ExecuteQueryRequest {
  connectionId: string;
  query: string;
  maxRows: number;
}

export interface ExecuteQueryResponse {
  success: boolean;
  errorMessage?: string;
  columnNames?: string[];
  rows?: Array<{ [key: string]: any }>;
  rowCount: number;
  executionTimeMs: number;
}

export interface SchemaInfo {
  tables?: TableInfo[];
  views?: ViewInfo[];
}

export interface TableInfo {
  name: string;
  schema: string;
  columns?: ColumnInfo[];
}

export interface ViewInfo {
  name: string;
  schema: string;
}

export interface ColumnInfo {
  name: string;
  dataType: string;
  isNullable: boolean;
  isPrimaryKey: boolean;
}
