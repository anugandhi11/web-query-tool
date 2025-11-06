import { Component, OnInit, OnDestroy, ViewChild, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DatabaseService } from '../../services/database.service';
import { DatabaseConnection, ExecuteQueryResponse } from '../../models/database.model';

declare const monaco: any;

@Component({
  selector: 'app-sql-editor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sql-editor.component.html',
  styleUrls: ['./sql-editor.component.scss']
})
export class SqlEditorComponent implements OnInit, OnDestroy {
  @ViewChild('editorContainer', { static: true }) editorContainer!: ElementRef;

  private databaseService = inject(DatabaseService);
  private editor: any;

  connections: DatabaseConnection[] = [];
  selectedConnectionId: string = '';
  query: string = 'SELECT * FROM ';
  executing: boolean = false;
  queryResult: ExecuteQueryResponse | null = null;
  error: string = '';

  ngOnInit(): void {
    this.loadConnections();
    this.initializeMonacoEditor();
  }

  ngOnDestroy(): void {
    if (this.editor) {
      this.editor.dispose();
    }
  }

  loadConnections(): void {
    this.databaseService.getConnections().subscribe({
      next: (connections) => {
        this.connections = connections;
        if (connections.length > 0 && !this.selectedConnectionId) {
          this.selectedConnectionId = connections[0].id || '';
        }
      },
      error: (error) => {
        console.error('Failed to load connections:', error);
        this.error = 'Failed to load database connections';
      }
    });
  }

  async initializeMonacoEditor(): Promise<void> {
    // Wait for Monaco to load
    await this.loadMonaco();

    // Register SQL language with monaco-sql-languages
    if (typeof (window as any).MonacoSQLLanguages !== 'undefined') {
      const monacoSQLLanguages = (window as any).MonacoSQLLanguages;

      // Setup MySQL language features
      monacoSQLLanguages.setupLanguageFeatures('mysql', {
        completionItems: {
          keywords: true,
          functions: true
        }
      });
    }

    // Create the editor
    this.editor = monaco.editor.create(this.editorContainer.nativeElement, {
      value: this.query,
      language: 'sql',
      theme: 'vs-dark',
      automaticLayout: true,
      fontSize: 14,
      minimap: { enabled: false },
      scrollBeyondLastLine: false,
      wordWrap: 'on',
      lineNumbers: 'on',
      roundedSelection: true,
      quickSuggestions: true,
      suggest: {
        showKeywords: true,
        showSnippets: true
      }
    });

    // Update query when editor changes
    this.editor.onDidChangeModelContent(() => {
      this.query = this.editor.getValue();
    });
  }

  private loadMonaco(): Promise<void> {
    return new Promise((resolve) => {
      if (typeof monaco !== 'undefined') {
        resolve();
        return;
      }

      const script = document.createElement('script');
      script.src = 'assets/monaco-editor/min/vs/loader.js';
      script.onload = () => {
        (window as any).require.config({ paths: { vs: 'assets/monaco-editor/min/vs' } });
        (window as any).require(['vs/editor/editor.main'], () => {
          resolve();
        });
      };
      document.body.appendChild(script);
    });
  }

  executeQuery(): void {
    if (!this.selectedConnectionId) {
      this.error = 'Please select a database connection';
      return;
    }

    if (!this.query.trim()) {
      this.error = 'Please enter a SQL query';
      return;
    }

    this.executing = true;
    this.error = '';
    this.queryResult = null;

    this.databaseService.executeQuery({
      connectionId: this.selectedConnectionId,
      query: this.query,
      maxRows: 1000
    }).subscribe({
      next: (result) => {
        this.executing = false;
        this.queryResult = result;
        if (!result.success) {
          this.error = result.errorMessage || 'Query execution failed';
        }
      },
      error: (error) => {
        this.executing = false;
        this.error = `Error: ${error.message || 'Query execution failed'}`;
        console.error('Query execution error:', error);
      }
    });
  }

  formatValue(value: any): string {
    if (value === null || value === undefined) {
      return 'NULL';
    }
    if (typeof value === 'object') {
      return JSON.stringify(value);
    }
    return String(value);
  }

  exportToCSV(): void {
    if (!this.queryResult || !this.queryResult.rows) {
      return;
    }

    const csv = this.convertToCSV(this.queryResult);
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `query-result-${Date.now()}.csv`;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  private convertToCSV(result: ExecuteQueryResponse): string {
    if (!result.columnNames || !result.rows) {
      return '';
    }

    const header = result.columnNames.join(',');
    const rows = result.rows.map(row =>
      result.columnNames!.map(col => {
        const value = row[col];
        if (value === null || value === undefined) return '';
        const str = String(value);
        return str.includes(',') || str.includes('"') || str.includes('\n')
          ? `"${str.replace(/"/g, '""')}"`
          : str;
      }).join(',')
    );

    return [header, ...rows].join('\n');
  }
}
