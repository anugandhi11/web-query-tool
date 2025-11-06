import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { ConnectionService } from '../../../core/services/connection.service';
import { ConnectionResponse, DatabaseType } from '../../../core/models/connection.models';

@Component({
  selector: 'app-connection-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatTableModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule
  ],
  template: `
    <div class="connections-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>Database Connections</mat-card-title>
          <mat-card-subtitle>Manage your database connections</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <button mat-raised-button color="primary" (click)="showAddConnectionForm()">
            <mat-icon>add</mat-icon>
            Add New Connection
          </button>

          <!-- Add/Edit Connection Form -->
          @if (showForm()) {
            <div class="connection-form">
              <h3>{{ editingConnection() ? 'Edit' : 'Add New' }} Connection</h3>
              <form [formGroup]="connectionForm" (ngSubmit)="saveConnection()">
                <div class="form-row">
                  <mat-form-field class="full-width">
                    <mat-label>Connection Name</mat-label>
                    <input matInput formControlName="name" required>
                  </mat-form-field>
                </div>

                <div class="form-row">
                  <mat-form-field class="full-width">
                    <mat-label>Database Type</mat-label>
                    <mat-select formControlName="type" required>
                      <mat-option value="PostgreSQL">PostgreSQL</mat-option>
                      <mat-option value="MySQL">MySQL</mat-option>
                      <mat-option value="SQLServer">SQL Server</mat-option>
                      <mat-option value="Redshift">AWS Redshift</mat-option>
                    </mat-select>
                  </mat-form-field>
                </div>

                <div class="form-row">
                  <mat-form-field style="flex: 3">
                    <mat-label>Host</mat-label>
                    <input matInput formControlName="host" required>
                  </mat-form-field>

                  <mat-form-field style="flex: 1">
                    <mat-label>Port</mat-label>
                    <input matInput type="number" formControlName="port" required>
                  </mat-form-field>
                </div>

                <div class="form-row">
                  <mat-form-field class="full-width">
                    <mat-label>Database Name</mat-label>
                    <input matInput formControlName="database" required>
                  </mat-form-field>
                </div>

                <div class="form-row">
                  <mat-form-field class="full-width">
                    <mat-label>Username</mat-label>
                    <input matInput formControlName="username" required>
                  </mat-form-field>
                </div>

                <div class="form-row">
                  <mat-form-field class="full-width">
                    <mat-label>Password</mat-label>
                    <input matInput type="password" formControlName="password"
                           [required]="!editingConnection()">
                    @if (editingConnection()) {
                      <mat-hint>Leave blank to keep existing password</mat-hint>
                    }
                  </mat-form-field>
                </div>

                <div class="form-row">
                  <mat-form-field class="full-width">
                    <mat-label>Query Timeout (seconds)</mat-label>
                    <input matInput type="number" formControlName="queryTimeoutSeconds" required>
                  </mat-form-field>
                </div>

                <div class="form-actions">
                  <button mat-raised-button type="button" (click)="testConnection()"
                          [disabled]="saving()">
                    <mat-icon>cable</mat-icon>
                    Test Connection
                  </button>
                  <button mat-raised-button color="primary" type="submit"
                          [disabled]="connectionForm.invalid || saving()">
                    <mat-icon>save</mat-icon>
                    Save
                  </button>
                  <button mat-button type="button" (click)="cancelForm()">
                    Cancel
                  </button>
                </div>

                @if (testResult()) {
                  <div [class]="testResult()!.success ? 'success-message' : 'error-message'">
                    <mat-icon>{{ testResult()!.success ? 'check_circle' : 'error' }}</mat-icon>
                    {{ testResult()!.message }} ({{ testResult()!.responseTimeMs }}ms)
                  </div>
                }
              </form>
            </div>
          }

          <!-- Connections Table -->
          <div class="connections-table">
            <table mat-table [dataSource]="connectionService.connections()" class="full-width">
              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef>Name</th>
                <td mat-cell *matCellDef="let conn">{{ conn.name }}</td>
              </ng-container>

              <ng-container matColumnDef="type">
                <th mat-header-cell *matHeaderCellDef>Type</th>
                <td mat-cell *matCellDef="let conn">
                  <mat-chip>{{ conn.type }}</mat-chip>
                </td>
              </ng-container>

              <ng-container matColumnDef="host">
                <th mat-header-cell *matHeaderCellDef>Host</th>
                <td mat-cell *matCellDef="let conn">{{ conn.host }}:{{ conn.port }}</td>
              </ng-container>

              <ng-container matColumnDef="database">
                <th mat-header-cell *matHeaderCellDef>Database</th>
                <td mat-cell *matCellDef="let conn">{{ conn.database }}</td>
              </ng-container>

              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Status</th>
                <td mat-cell *matCellDef="let conn">
                  @if (conn.lastTestedAt) {
                    <mat-chip [color]="conn.lastTestSuccessful ? 'primary' : 'warn'">
                      {{ conn.lastTestSuccessful ? 'Connected' : 'Failed' }}
                    </mat-chip>
                  } @else {
                    <mat-chip>Not Tested</mat-chip>
                  }
                </td>
              </ng-container>

              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef>Actions</th>
                <td mat-cell *matCellDef="let conn">
                  <button mat-icon-button (click)="editConnection(conn)">
                    <mat-icon>edit</mat-icon>
                  </button>
                  <button mat-icon-button color="warn" (click)="deleteConnection(conn.id)">
                    <mat-icon>delete</mat-icon>
                  </button>
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
    .connections-container {
      max-width: 1200px;
      margin: 0 auto;
    }

    .connection-form {
      margin: 24px 0;
      padding: 24px;
      background-color: #f5f5f5;
      border-radius: 8px;
    }

    .form-row {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
    }

    .form-actions {
      display: flex;
      gap: 12px;
      margin-top: 24px;
    }

    .connections-table {
      margin-top: 24px;
    }

    .success-message, .error-message {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px;
      margin-top: 16px;
      border-radius: 4px;
    }

    .success-message {
      background-color: #e8f5e9;
      color: #2e7d32;
      border-left: 4px solid #4caf50;
    }

    .error-message {
      background-color: #ffebee;
      color: #c62828;
      border-left: 4px solid #f44336;
    }
  `]
})
export class ConnectionListComponent {
  private readonly fb = inject(FormBuilder);
  readonly connectionService = inject(ConnectionService);

  showForm = signal(false);
  saving = signal(false);
  editingConnection = signal<ConnectionResponse | null>(null);
  testResult = signal<any>(null);

  displayedColumns = ['name', 'type', 'host', 'database', 'status', 'actions'];

  connectionForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    type: ['PostgreSQL', Validators.required],
    host: ['localhost', Validators.required],
    port: [5432, [Validators.required, Validators.min(1), Validators.max(65535)]],
    database: ['', Validators.required],
    username: ['', Validators.required],
    password: [''],
    queryTimeoutSeconds: [60, [Validators.required, Validators.min(10), Validators.max(300)]]
  });

  ngOnInit(): void {
    this.connectionService.getConnections().subscribe();

    // Update port based on database type
    this.connectionForm.get('type')?.valueChanges.subscribe(type => {
      const defaultPorts: any = {
        'PostgreSQL': 5432,
        'MySQL': 3306,
        'SQLServer': 1433,
        'Redshift': 5439
      };
      this.connectionForm.patchValue({ port: defaultPorts[type] || 5432 });
    });
  }

  showAddConnectionForm(): void {
    this.showForm.set(true);
    this.editingConnection.set(null);
    this.testResult.set(null);
    this.connectionForm.reset({
      type: 'PostgreSQL',
      host: 'localhost',
      port: 5432,
      queryTimeoutSeconds: 60
    });
  }

  editConnection(conn: ConnectionResponse): void {
    this.showForm.set(true);
    this.editingConnection.set(conn);
    this.testResult.set(null);
    this.connectionForm.patchValue({
      name: conn.name,
      type: conn.type,
      host: conn.host,
      port: conn.port,
      database: conn.database,
      username: conn.username,
      password: '',
      queryTimeoutSeconds: conn.queryTimeoutSeconds
    });
  }

  cancelForm(): void {
    this.showForm.set(false);
    this.editingConnection.set(null);
    this.testResult.set(null);
    this.connectionForm.reset();
  }

  testConnection(): void {
    if (this.connectionForm.invalid) return;

    this.saving.set(true);
    this.testResult.set(null);

    this.connectionService.testNewConnection(this.connectionForm.value).subscribe({
      next: (result) => {
        this.saving.set(false);
        this.testResult.set(result);
      },
      error: (error) => {
        this.saving.set(false);
        this.testResult.set({
          success: false,
          message: error.error?.message || 'Connection test failed',
          responseTimeMs: 0
        });
      }
    });
  }

  saveConnection(): void {
    if (this.connectionForm.invalid) return;

    this.saving.set(true);

    const editing = this.editingConnection();
    const request = this.connectionForm.value;

    const operation = editing
      ? this.connectionService.updateConnection(editing.id, request)
      : this.connectionService.createConnection(request);

    operation.subscribe({
      next: () => {
        this.saving.set(false);
        this.cancelForm();
      },
      error: (error) => {
        this.saving.set(false);
        alert(error.error?.error || 'Failed to save connection');
      }
    });
  }

  deleteConnection(id: string): void {
    if (!confirm('Are you sure you want to delete this connection?')) return;

    this.connectionService.deleteConnection(id).subscribe({
      error: (error) => {
        alert(error.error?.error || 'Failed to delete connection');
      }
    });
  }
}
