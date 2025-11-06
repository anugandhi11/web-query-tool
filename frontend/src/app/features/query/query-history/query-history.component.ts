import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { QueryService } from '../../../core/services/query.service';
import { QueryHistory } from '../../../core/models/query.models';

@Component({
  selector: 'app-query-history',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule
  ],
  template: `
    <div class="history-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>Query History</mat-card-title>
          <mat-card-subtitle>View your recent query executions</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <button mat-raised-button (click)="loadHistory()">
            <mat-icon>refresh</mat-icon>
            Refresh
          </button>

          <div class="history-table">
            <table mat-table [dataSource]="history()" class="full-width">
              <ng-container matColumnDef="executedAt">
                <th mat-header-cell *matHeaderCellDef>Executed</th>
                <td mat-cell *matCellDef="let item">
                  {{ item.executedAt | date:'short' }}
                </td>
              </ng-container>

              <ng-container matColumnDef="sqlPreview">
                <th mat-header-cell *matHeaderCellDef>Query Preview</th>
                <td mat-cell *matCellDef="let item" class="sql-preview">
                  <code>{{ item.sqlPreview }}</code>
                </td>
              </ng-container>

              <ng-container matColumnDef="submissionType">
                <th mat-header-cell *matHeaderCellDef>Type</th>
                <td mat-cell *matCellDef="let item">
                  <mat-chip>{{ item.submissionType }}</mat-chip>
                </td>
              </ng-container>

              <ng-container matColumnDef="executionTime">
                <th mat-header-cell *matHeaderCellDef>Time</th>
                <td mat-cell *matCellDef="let item">
                  {{ item.executionTimeMs }}ms
                </td>
              </ng-container>

              <ng-container matColumnDef="rowCount">
                <th mat-header-cell *matHeaderCellDef>Rows</th>
                <td mat-cell *matCellDef="let item">
                  {{ item.rowCount }}
                </td>
              </ng-container>

              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Status</th>
                <td mat-cell *matCellDef="let item">
                  <mat-chip [color]="item.success ? 'primary' : 'warn'">
                    <mat-icon>{{ item.success ? 'check_circle' : 'error' }}</mat-icon>
                    {{ item.success ? 'Success' : 'Failed' }}
                  </mat-chip>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
            </table>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .history-container {
      max-width: 1400px;
      margin: 0 auto;
    }

    .history-table {
      margin-top: 24px;
      overflow: auto;
    }

    .sql-preview {
      max-width: 400px;
    }

    .sql-preview code {
      display: block;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      background-color: #f5f5f5;
      padding: 4px 8px;
      border-radius: 4px;
      font-family: 'Courier New', monospace;
      font-size: 12px;
    }

    mat-chip {
      display: inline-flex;
      align-items: center;
      gap: 4px;
    }
  `]
})
export class QueryHistoryComponent {
  private readonly queryService = inject(QueryService);

  history = signal<QueryHistory[]>([]);
  displayedColumns = ['executedAt', 'sqlPreview', 'submissionType', 'executionTime', 'rowCount', 'status'];

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    this.queryService.getQueryHistory(50).subscribe({
      next: (data) => {
        this.history.set(data);
      },
      error: (error) => {
        console.error('Failed to load history:', error);
      }
    });
  }
}
