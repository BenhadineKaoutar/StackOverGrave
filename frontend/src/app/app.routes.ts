import { Routes } from '@angular/router';
import { GraveyardDashboardComponent } from './components/graveyard-dashboard/graveyard-dashboard.component';
import { CodeViewerComponent } from './components/code-viewer/code-viewer.component';

export const routes: Routes = [
  {
    path: '',
    component: GraveyardDashboardComponent
  },
  {
    path: 'project/:id',
    component: CodeViewerComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];
