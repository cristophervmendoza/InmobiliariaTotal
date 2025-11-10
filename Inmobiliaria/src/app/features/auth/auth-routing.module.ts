// features/auth/auth-routing.module.ts
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Login } from './pages/login/login';
// Opcional: página de recuperar contraseña (crea el componente si lo usarás)
import { Register } from './pages/register/register';
import { Forgotpassword } from './pages/forgotpassword/forgotpassword';
import { loginRedirectGuard } from '../../core/guards/login-redirect.guard';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: Login,
    canActivate: [loginRedirectGuard] // ✅ Guard agregado
  },
  {
    path: 'register',
    component: Register,
    canActivate: [loginRedirectGuard] // ✅ Guard agregado (opcional)
  },
  {
    path: 'forgot-password',
    component: Forgotpassword, // Si tienes esta ruta
    canActivate: [loginRedirectGuard] // ✅ Guard agregado (opcional)
  }
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule { }
