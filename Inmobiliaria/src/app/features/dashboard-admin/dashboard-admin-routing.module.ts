import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Layout } from './layout/layout';

// Pages
import { Analysis } from './pages/analysis/analysis';
import { Blockedaccounts } from './pages/blockedaccounts/blockedaccounts';
import { Companies } from './pages/companies/companies';
import { Dashboard } from './pages/dashboard/dashboard';
import { Employees } from './pages/employees/employees';
import { Maintenance } from './pages/maintenance/maintenance';
import { Management } from './pages/management/management';
import { PropertiesAdmin } from './pages/properties-admin/properties-admin';
import { Quotes } from './pages/quotes/quotes';
import { Reports } from './pages/reports/reports';
import { Requestadvisors } from './pages/requestadvisors/requestadvisors';
import { TestimonialsComponent } from './pages/testimonials/testimonials';  // ← CORREGIDO
import { Configurations } from './pages/configurations/configurations';
import { Profile } from './pages/profile/profile';

// Guard para todos los hijos
import { roleCanActivateChildGuard } from '../../core/guards/role-can-activate-child.guard';

const routes: Routes = [
  {
    path: '',
    component: Layout,
    data: { roles: ['admin'] },
    canActivateChild: [roleCanActivateChildGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: Dashboard },
      { path: 'employees', component: Employees },
      { path: 'reports', component: Reports },
      { path: 'properties', component: PropertiesAdmin },
      { path: 'companies', component: Companies },
      { path: 'analysis', component: Analysis },
      { path: 'blockedaccounts', component: Blockedaccounts },
      { path: 'maintenance', component: Maintenance },
      { path: 'management', component: Management },
      { path: 'quotes', component: Quotes },
      { path: 'requestadvisors', component: Requestadvisors },
      { path: 'testimonials', component: TestimonialsComponent },  // ← CORREGIDO
      { path: 'configurations', component: Configurations },
      { path: 'profile', component: Profile },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardAdminRoutingModule { }
