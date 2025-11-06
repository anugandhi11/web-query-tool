import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WebQueryService } from '../../services/web-query.service';
import { QueryType, WebQueryRequest, WebQueryResponse } from '../../models/web-query.model';

@Component({
  selector: 'app-query-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './query-form.component.html',
  styleUrls: ['./query-form.component.scss']
})
export class QueryFormComponent implements OnInit {
  private webQueryService = inject(WebQueryService);

  url: string = '';
  selector: string = '';
  queryType: QueryType = QueryType.Text;
  loading: boolean = false;
  response: WebQueryResponse | null = null;
  error: string = '';

  queryTypes = [
    { value: QueryType.HtmlContent, label: 'HTML Content' },
    { value: QueryType.Text, label: 'Text Content' },
    { value: QueryType.Links, label: 'Links' },
    { value: QueryType.Images, label: 'Images' },
    { value: QueryType.Metadata, label: 'Metadata' }
  ];

  ngOnInit(): void {
    this.checkApiHealth();
  }

  checkApiHealth(): void {
    this.webQueryService.checkHealth().subscribe({
      next: (response) => {
        console.log('API is healthy:', response);
      },
      error: (error) => {
        console.error('API health check failed:', error);
        this.error = 'Cannot connect to API server. Please ensure the backend is running.';
      }
    });
  }

  onSubmit(): void {
    if (!this.url) {
      this.error = 'Please enter a URL';
      return;
    }

    this.loading = true;
    this.error = '';
    this.response = null;

    const request: WebQueryRequest = {
      url: this.url,
      selector: this.selector || undefined,
      queryType: this.queryType
    };

    this.webQueryService.query(request).subscribe({
      next: (response) => {
        this.loading = false;
        this.response = response;
        if (!response.success) {
          this.error = response.errorMessage || 'Query failed';
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = `Error: ${error.message || 'Failed to query the URL'}`;
        console.error('Query error:', error);
      }
    });
  }

  clear(): void {
    this.url = '';
    this.selector = '';
    this.queryType = QueryType.Text;
    this.response = null;
    this.error = '';
  }

  get hasResults(): boolean {
    return this.response !== null && this.response.success;
  }

  get displayContent(): string {
    if (!this.response) return '';

    if (this.response.content) {
      return this.response.content;
    }

    if (this.response.items && this.response.items.length > 0) {
      return this.response.items.join('\n\n');
    }

    return '';
  }

  get metadataEntries(): Array<{ key: string; value: string }> {
    if (!this.response?.metadata) return [];
    return Object.entries(this.response.metadata).map(([key, value]) => ({
      key,
      value
    }));
  }

  copyToClipboard(): void {
    const content = this.displayContent;
    navigator.clipboard.writeText(content).then(() => {
      alert('Content copied to clipboard!');
    });
  }
}
