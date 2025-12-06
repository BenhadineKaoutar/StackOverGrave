import { Routes } from '@angular/router';
import { GraveyardDashboardComponent } from './components/graveyard-dashboard/graveyard-dashboard.component';
import { CodeViewerComponent } from './components/code-viewer/code-viewer.component';
import { RepositoryImportComponent } from './components/repository-import/repository-import.component';
import { ConversionStatusComponent } from './components/conversion-status/conversion-status.component';

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
    path: 'repository/import',
    component: RepositoryImportComponent
  },
  {
    path: 'repository/status/:id',
    component: ConversionStatusComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];
