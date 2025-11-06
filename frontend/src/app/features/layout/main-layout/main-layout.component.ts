import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    RouterOutlet,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule,
    MatMenuModule
  ],
  template: `
    <mat-sidenav-container class="sidenav-container">
      <mat-sidenav mode="side" opened class="sidenav">
        <mat-nav-list>
          <a mat-list-item routerLink="/query" routerLinkActive="active-link">
            <mat-icon>code</mat-icon>
            <span>Query Editor</span>
          </a>
          <a mat-list-item routerLink="/connections" routerLinkActive="active-link">
            <mat-icon>storage</mat-icon>
            <span>Connections</span>
          </a>
          <a mat-list-item routerLink="/history" routerLinkActive="active-link">
            <mat-icon>history</mat-icon>
            <span>Query History</span>
          </a>
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content>
        <mat-toolbar color="primary">
          <span>Web Query Tool</span>
          <span class="spacer"></span>

          @if (authService.currentUser(); as user) {
            <button mat-button [matMenuTriggerFor]="userMenu">
              <mat-icon>account_circle</mat-icon>
              {{ user.fullName }}
            </button>
            <mat-menu #userMenu="matMenu">
              <button mat-menu-item disabled>
                <mat-icon>email</mat-icon>
                {{ user.email }}
              </button>
              <button mat-menu-item disabled>
                <mat-icon>badge</mat-icon>
                {{ user.role }}
              </button>
              <mat-divider></mat-divider>
              <button mat-menu-item (click)="logout()">
                <mat-icon>logout</mat-icon>
                Logout
              </button>
            </mat-menu>
          }
        </mat-toolbar>

        <div class="content">
          <router-outlet></router-outlet>
        </div>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .sidenav-container {
      height: 100vh;
    }

    .sidenav {
      width: 250px;
      padding-top: 20px;
    }

    .active-link {
      background-color: rgba(0, 0, 0, 0.04);
    }

    mat-nav-list a {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .content {
      padding: 20px;
      min-height: calc(100vh - 64px);
    }

    .spacer {
      flex: 1 1 auto;
    }

    mat-divider {
      margin: 8px 0;
    }
  `]
})
export class MainLayoutComponent {
  readonly authService = inject(AuthService);

  logout(): void {
    this.authService.logout();
  }
}
