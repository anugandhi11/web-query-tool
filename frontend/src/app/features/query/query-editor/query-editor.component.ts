import { Component, inject, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import { QueryService } from '../../../core/services/query.service';
import { ConnectionService } from '../../../core/services/connection.service';
import { QueryResult, QueryExecutionOptions } from '../../../core/models/query.models';
import { ConnectionResponse } from '../../../core/models/connection.models';

@Component({
  selector: 'app-query-editor',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatChipsModule,
    MonacoEditorModule
  ],
  template: `
    <div class="query-editor-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>SQL Query Editor</mat-card-title>
          <mat-card-subtitle>WAF-Friendly Query Execution with Base64 Encoding</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <!-- Connection Selection -->
          <mat-form-field class="full-width">
            <mat-label>Select Database Connection</mat-label>
            <mat-select [(value)]="selectedConnectionId" (selectionChange)="onConnectionChange()">
              @for (conn of connectionService.connections(); track conn.id) {
                <mat-option [value]="conn.id">
                  {{ conn.name }} ({{ conn.type }})
                </mat-option>
              }
            </mat-select>
          </mat-form-field>

          <!-- Monaco SQL Editor -->
          <div class="editor-wrapper">
            <div class="editor-toolbar">
              <button mat-raised-button color="primary" (click)="executeQuery()"
                      [disabled]="!selectedConnectionId || loading()">
                <mat-icon>play_arrow</mat-icon>
                Execute (Ctrl+Enter)
              </button>
              <button mat-button (click)="clearEditor()">
                <mat-icon>clear</mat-icon>
                Clear
              </button>
              <mat-chip-set>
                <mat-chip>
                  <mat-icon>security</mat-icon>
                  Base64 Encoded
                </mat-chip>
              </mat-chip-set>
            </div>

            <ngx-monaco-editor
              class="monaco-editor"
              [options]="editorOptions"
              [(ngModel)]="sqlQuery"
              (ngModelChange)="onEditorChange($event)">
            </ngx-monaco-editor>
          </div>

          <!-- Loading Spinner -->
          @if (loading()) {
            <div class="loading-spinner">
              <mat-spinner></mat-spinner>
              <p>Executing query...</p>
            </div>
          }

          <!-- Error Message -->
          @if (errorMessage()) {
            <div class="error-message">
              <mat-icon>error</mat-icon>
              {{ errorMessage() }}
            </div>
          }

          <!-- Query Results -->
          @if (queryResult() && !loading()) {
            <div class="results-section">
              <div class="results-header">
                <h3>Query Results</h3>
                <div class="results-info">
                  <mat-chip-set>
                    <mat-chip>
                      <mat-icon>table_rows</mat-icon>
                      {{ queryResult()!.rowCount }} rows
                    </mat-chip>
                    <mat-chip>
                      <mat-icon>timer</mat-icon>
                      {{ queryResult()!.executionTimeMs }}ms
                    </mat-chip>
                    @if (queryResult()!.hasMoreRows) {
                      <mat-chip color="warn">
                        <mat-icon>warning</mat-icon>
                        Results truncated
                      </mat-chip>
                    }
                  </mat-chip-set>
                </div>
              </div>

              <div class="table-container">
                <table mat-table [dataSource]="getTableDataSource()" class="results-table">
                  <!-- Dynamic Columns -->
                  @for (column of queryResult()!.columns; track column) {
                    <ng-container [matColumnDef]="column">
                      <th mat-header-cell *matHeaderCellDef>{{ column }}</th>
                      <td mat-cell *matCellDef="let row">
                        {{ formatCellValue(row[column]) }}
                      </td>
                    </ng-container>
                  }

                  <tr mat-header-row *matHeaderRowDef="queryResult()!.columns"></tr>
                  <tr mat-row *matRowDef="let row; columns: queryResult()!.columns"></tr>
                </table>
              </div>
            </div>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .query-editor-container {
      max-width: 1400px;
      margin: 0 auto;
    }

    mat-card {
      margin: 0;
    }

    .editor-wrapper {
      margin-top: 20px;
    }

    .editor-toolbar {
      display: flex;
      gap: 12px;
      align-items: center;
      margin-bottom: 12px;
      padding: 12px;
      background-color: #f5f5f5;
      border-radius: 4px;
    }

    .monaco-editor {
      height: 400px;
      border: 1px solid #ddd;
      border-radius: 4px;
    }

    .loading-spinner {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 40px;
      gap: 16px;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 16px;
      margin-top: 16px;
      background-color: #ffebee;
      border-left: 4px solid #f44336;
      border-radius: 4px;
      color: #c62828;
    }

    .results-section {
      margin-top: 24px;
    }

    .results-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
    }

    .results-info {
      display: flex;
      gap: 8px;
    }

    .table-container {
      max-height: 500px;
      overflow: auto;
      border: 1px solid #ddd;
      border-radius: 4px;
    }

    .results-table {
      width: 100%;
    }

    .results-table th {
      background-color: #3f51b5;
      color: white;
      font-weight: 600;
      position: sticky;
      top: 0;
      z-index: 10;
    }

    .results-table td {
      padding: 12px;
      border-bottom: 1px solid #e0e0e0;
    }

    .results-table tr:hover {
      background-color: #f5f5f5;
    }
  `]
})
export class QueryEditorComponent {
  private readonly fb = inject(FormBuilder);
  readonly queryService = inject(QueryService);
  readonly connectionService = inject(ConnectionService);

  // Signals for reactive state
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  queryResult = signal<QueryResult | null>(null);

  selectedConnectionId: string | null = null;
  sqlQuery: string = '-- Enter your SQL query here\nSELECT * FROM employees LIMIT 10;';

  editorOptions = {
    theme: 'vs',
    language: 'sql',
    automaticLayout: true,
    fontSize: 14,
    minimap: { enabled: true },
    scrollBeyondLastLine: false,
    wordWrap: 'on'
  };

  ngOnInit(): void {
    // Load connections
    this.connectionService.getConnections().subscribe();
  }

  onConnectionChange(): void {
    this.errorMessage.set(null);
    this.queryResult.set(null);
  }

  onEditorChange(value: string): void {
    this.sqlQuery = value;
  }

  executeQuery(): void {
    if (!this.selectedConnectionId) {
      this.errorMessage.set('Please select a database connection');
      return;
    }

    if (!this.sqlQuery.trim()) {
      this.errorMessage.set('Please enter a SQL query');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);
    this.queryResult.set(null);

    // Encode SQL to Base64 (WAF-FRIENDLY!)
    const encodedQuery = this.queryService.encodeSqlQuery(this.sqlQuery);

    const request = {
      queryData: encodedQuery,
      connectionId: this.selectedConnectionId,
      options: {
        maxRows: 1000,
        timeoutSeconds: 60,
        includeMetadata: true
      } as QueryExecutionOptions
    };

    this.queryService.executeEncodedQuery(request).subscribe({
      next: (result) => {
        this.loading.set(false);
        if (result.success) {
          this.queryResult.set(result);
        } else {
          this.errorMessage.set(result.errorMessage || 'Query execution failed');
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.errorMessage.set(error.error?.detail || error.error?.error || 'An error occurred');
      }
    });
  }

  clearEditor(): void {
    this.sqlQuery = '';
    this.queryResult.set(null);
    this.errorMessage.set(null);
  }

  getTableDataSource(): any[] {
    const result = this.queryResult();
    if (!result) return [];

    return result.rows.map(row => {
      const obj: any = {};
      result.columns.forEach((col, index) => {
        obj[col] = row[index];
      });
      return obj;
    });
  }

  formatCellValue(value: any): string {
    if (value === null || value === undefined) {
      return '<NULL>';
    }
    if (typeof value === 'object') {
      return JSON.stringify(value);
    }
    return String(value);
  }
}
