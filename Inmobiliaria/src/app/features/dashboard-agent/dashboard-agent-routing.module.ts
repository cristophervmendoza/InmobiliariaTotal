import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Layout } from './layout/layout';

// Pages

import { AgendaAgent } from './pages/agenda-agent/agenda-agent';
import { CatalogAgent } from './pages/catalog-agent/catalog-agent';
import { CompaniesAgent } from './pages/companies-agent/companies-agent';
import { DashboardAgent } from './pages/dashboard-agent/dashboard-agent';
import { MaintenanceAgent } from './pages/maintenance-agent/maintenance-agent';
import { MessagesAgent } from './pages/messages-agent/messages-agent';
import { PropertiesAgent } from './pages/properties-agent/properties-agent';
import { ReportsAgent } from './pages/reports-agent/reports-agent';

// Extras
import { ConfigurationsAgent } from './pages/configurations-agent/configurations-agent';
import { ProfileAgent } from './pages/profile-agent/profile-agent';

import { roleCanActivateChildGuard } from '../../core/guards/role-can-activate-child.guard';


const routes: Routes = [
  {
    path: '',
    component: Layout,
    data: { roles: ['agent'] },
    canActivateChild: [roleCanActivateChildGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardAgent },
      { path: 'agenda', component: AgendaAgent },
      { path: 'reports', component: ReportsAgent },
      { path: 'properties', component: PropertiesAgent },
      { path: 'companies', component: CompaniesAgent },
      { path: 'catalog', component: CatalogAgent },
      { path: 'messages', component: MessagesAgent },
      { path: 'maintenance', component: MaintenanceAgent },
      { path: 'configurations', component: ConfigurationsAgent },
      { path: 'profile', component: ProfileAgent },



    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardAgentRoutingModule { }
