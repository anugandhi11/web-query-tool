import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./features/layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: '',
        redirectTo: 'query',
        pathMatch: 'full'
      },
      {
        path: 'query',
        loadComponent: () => import('./features/query/query-editor/query-editor.component').then(m => m.QueryEditorComponent)
      },
      {
        path: 'connections',
        loadComponent: () => import('./features/connections/connection-list/connection-list.component').then(m => m.ConnectionListComponent)
      },
      {
        path: 'history',
        loadComponent: () => import('./features/query/query-history/query-history.component').then(m => m.QueryHistoryComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
