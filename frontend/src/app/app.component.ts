import { Component } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule],
  template: `
    <div class="app-container">
      <header class="app-header" *ngIf="showHeader">
        <h1 (click)="goHome()">StackOverGrave</h1>
        <p class="subtitle">Where Legacy Code Rests in Peace</p>
      </header>
      <router-outlet></router-outlet>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      padding: 2rem;
    }

    .app-header {
      text-align: center;
      margin-bottom: 3rem;
    }

    h1 {
      font-size: 48px;
      margin-bottom: 0.5rem;
      text-shadow: 0 0 20px rgba(0, 255, 65, 0.5);
      cursor: pointer;
      transition: all 300ms ease-out;

      &:hover {
        text-shadow: 0 0 30px rgba(0, 255, 65, 0.8);
        transform: scale(1.05);
      }
    }

    .subtitle {
      color: var(--text-secondary);
      font-size: 18px;
    }

    @media (max-width: 768px) {
      .app-container {
        padding: 1rem;
      }

      h1 {
        font-size: 32px;
      }

      .subtitle {
        font-size: 14px;
      }
    }
  `]
})
export class AppComponent {
  title = 'StackOverGrave';
  showHeader = true;

  constructor(private router: Router) {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.showHeader = event.url === '/';
      });
  }

  goHome(): void {
    this.router.navigate(['/']);
  }
}
