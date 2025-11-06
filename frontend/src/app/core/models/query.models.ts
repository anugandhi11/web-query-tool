export interface QueryExecuteRequest {
  queryData: string;  // Base64 encoded SQL
  connectionId: string;
  options?: QueryExecutionOptions;
}

export interface QueryExecutionOptions {
  maxRows: number;
  timeoutSeconds: number;
  includeMetadata: boolean;
}

export interface QueryResult {
  success: boolean;
  columns: string[];
  columnMetadata?: ColumnMetadata[];
  rows: any[][];
  rowCount: number;
  executionTimeMs: number;
  errorMessage?: string;
  hasMoreRows: boolean;
}

export interface ColumnMetadata {
  name: string;
  dataType: string;
  isNullable: boolean;
  maxLength?: number;
  precision?: number;
  scale?: number;
}

export interface QueryBuilderRequest {
  connectionId: string;
  table: string;
  columns: string[];
  filters: QueryFilter[];
  orderBy?: QueryOrderBy;
  limit: number;
  offset: number;
}

export interface QueryFilter {
  column: string;
  operator: FilterOperator;
  value?: any;
}

export enum FilterOperator {
  Equals = 'Equals',
  NotEquals = 'NotEquals',
  GreaterThan = 'GreaterThan',
  GreaterThanOrEqual = 'GreaterThanOrEqual',
  LessThan = 'LessThan',
  LessThanOrEqual = 'LessThanOrEqual',
  Like = 'Like',
  NotLike = 'NotLike',
  In = 'In',
  NotIn = 'NotIn',
  IsNull = 'IsNull',
  IsNotNull = 'IsNotNull'
}

export interface QueryOrderBy {
  column: string;
  direction: SortDirection;
}

export enum SortDirection {
  Ascending = 'Ascending',
  Descending = 'Descending'
}

export interface QueryHistory {
  id: string;
  connectionId: string;
  sqlPreview: string;
  submissionType: QuerySubmissionType;
  executedAt: string;
  executionTimeMs: number;
  rowCount: number;
  success: boolean;
  errorMessage?: string;
}

export enum QuerySubmissionType {
  EncodedQuery = 'EncodedQuery',
  QueryBuilder = 'QueryBuilder',
  Template = 'Template',
  Direct = 'Direct'
}
