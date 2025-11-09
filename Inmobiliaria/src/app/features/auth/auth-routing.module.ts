// features/auth/auth-routing.module.ts
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Login } from './pages/login/login';
// Opcional: página de recuperar contraseña (crea el componente si lo usarás)
import { Register } from './pages/register/register';
import { Forgotpassword } from './pages/forgotpassword/forgotpassword';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'forgotpassword', component: Forgotpassword }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule { }
