import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { QueryFormComponent } from './components/query-form/query-form.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, QueryFormComponent],
  template: `
    <div class="container">
      <h1>🔍 Web Query Tool</h1>
      <app-query-form></app-query-form>
    </div>
  `,
  styles: []
})
export class AppComponent {
  title = 'Web Query Tool';
}
