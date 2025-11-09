// src/app/app-routing.module.ts
import { NgModule } from '@angular/core';
import { RouterModule, Routes, PreloadAllModules } from '@angular/router';
import { authCanLoadGuard } from './core/guards/auth-can-load.guard';
import { authCanActivateGuard } from './core/guards/auth-can-activate.guard';
import { NotFound } from './shared/components/not-found/not-found';
import { Dbconnection } from './dbconnection/dbconnection'; // <-- importa

const routes: Routes = [
  { path: '', loadChildren: () => import('./features/public/public.module').then(m => m.PublicModule) },
  { path: 'auth', loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule) },

  // Página de prueba de conexión (sin guards)
  { path: 'db-connection', component: Dbconnection },

  {
    path: 'admin',
    data: { roles: ['admin'] },
    canLoad: [authCanLoadGuard],
    canActivate: [authCanActivateGuard],
    loadChildren: () => import('./features/dashboard-admin/dashboard-admin.module')
      .then(m => m.DashboardAdminModule)
  },
  {
    path: 'agent',
    data: { roles: ['agent'] },
    canLoad: [authCanLoadGuard],
    canActivate: [authCanActivateGuard],
    loadChildren: () => import('./features/dashboard-agent/dashboard-agent.module')
      .then(m => m.DashboardAgentModule)
  },
  {
    path: 'client',
    data: { roles: ['client'] },
    canLoad: [authCanLoadGuard],
    canActivate: [authCanActivateGuard],
    loadChildren: () => import('./features/dashboard-client/dashboard-client.module')
      .then(m => m.DashboardClientModule)
  },

  { path: '404', component: NotFound },
  { path: '**', redirectTo: '/404' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
    preloadingStrategy: PreloadAllModules,
    enableTracing: false,
    useHash: false,
    onSameUrlNavigation: 'reload'
  })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
