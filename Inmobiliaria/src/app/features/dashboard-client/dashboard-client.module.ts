import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { DashboardClientRoutingModule } from './dashboard-client-routing.module';

// Layout
import { Layout } from './layout/layout';

// UI propias
import { NavbarClient } from './components/navbar-client/navbar-client';

// Pages
import { DashboardClient } from './pages/dashboard-client/dashboard-client';

// Extras
import { ConfigurationsClient } from './pages/configurations-client/configurations-client';
import { ProfileClient } from './pages/profile-client/profile-client';

// Iconos (lucide-angular)
import {
  LucideAngularModule,
  // Base
  Home,
  User,
  Settings,
  Bell,
  Search,
  // Menú/acciones
  Menu,
  X,
  ChevronDown,
  ChevronRight,
  LogOut,
  // Contenido
  FileText,
  BarChart3,
  Briefcase,
  Building2,
  Calendar,
  Users,
  Wrench,
  MessageSquare,
  IdCard,
  BriefcaseBusiness,
  Lock,
  AlertCircle,
  Clock,
  CheckCircle,
  Send,
  Edit,
  List,
  History,
  BookOpen,
  Hammer,
  Upload,
  File,
  Archive
} from 'lucide-angular';

@NgModule({
  declarations: [
    Layout,
    NavbarClient,
    DashboardClient,
    ConfigurationsClient,
    ProfileClient
  ],
  imports: [
    CommonModule,
    RouterModule,
    DashboardClientRoutingModule,
    LucideAngularModule.pick({
      // Base
      Home, User, Settings, Bell, Search,
      // Menú/acciones
      Menu, X, ChevronDown, ChevronRight, LogOut,
      // Contenido
      FileText, BarChart3, Briefcase, Building2, Calendar, Users, Wrench, MessageSquare,
      IdCard, BriefcaseBusiness, Lock,
      AlertCircle, Clock, CheckCircle, Send, Edit, List, History, BookOpen, Hammer,
      Upload, File, Archive
    })
  ]
})
export class DashboardClientModule { }
