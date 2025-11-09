import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AuthRoutingModule } from './auth-routing.module';

// Pages
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Forgotpassword } from './pages/forgotpassword/forgotpassword';

// Lucide
import {
  LucideAngularModule,
  Building2,
  Mail,
  Lock,
  ArrowLeft,
  User,
  Eye,
  EyeOff, // 👈 faltaba
  LogIn,
  Facebook,
  Twitter,
  Phone,
  MapPin,
  BookOpen,
  TrendingUp,
  Upload,
  FileText,
  X,
  AlertCircle // 👈 faltaba
} from 'lucide-angular';

@NgModule({
  declarations: [
    Login,
    Register,
    Forgotpassword
  ],
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    AuthRoutingModule,
    LucideAngularModule.pick({
      Building2,
      Mail,
      Lock,
      Eye,
      EyeOff, // 👈 habilita name="eye-off"
      LogIn,
      ArrowLeft,
      User,
      Facebook,
      Twitter,
      Phone,
      MapPin,
      BookOpen,
      TrendingUp,
      Upload,
      FileText,
      X,
      AlertCircle // 👈 habilita name="alert-circle"
    })
  ]
})
export class AuthModule { }
