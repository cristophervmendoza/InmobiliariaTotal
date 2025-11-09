import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Layout } from './layout/layout';

// Pages


import { DashboardClient } from './pages/dashboard-client/dashboard-client';

// Extras
import { ConfigurationsClient } from './pages/configurations-client/configurations-client';
import { ProfileClient } from './pages/profile-client/profile-client';
import { roleCanActivateChildGuard } from '../../core/guards/role-can-activate-child.guard';


const routes: Routes = [
  {
    path: '',
    component: Layout,
    data: { roles: ['client'] },
    canActivateChild: [roleCanActivateChildGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardClient },
 
      { path: 'configurations', component: ConfigurationsClient },
      { path: 'profile', component: ProfileClient },



    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardClientRoutingModule { }
