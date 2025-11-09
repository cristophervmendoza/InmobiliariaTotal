import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { DashboardAdminRoutingModule } from './dashboard-admin-routing.module';

// Layout
import { Layout } from './layout/layout';

// UI propias
import { Navbar } from './components/navbar/navbar';
import { Sidebar } from './components/sidebar/sidebar';

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
import { Testimonials } from './pages/testimonials/testimonials';

// Extras
import { Configurations } from './pages/configurations/configurations';
import { Profile } from './pages/profile/profile';

// Iconos (lucide-angular)
import {
  LucideAngularModule,
  // Básicos / layout
  Home,
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
  Settings,
  Bell,
  Search,
  User,
  LogOut,
  TrendingUp,
  DollarSign,
  PieChart,
  FileDown,
  Loader2,
  Star,
  CheckCircle2,
  XCircle,
  Eye,

  // Properties / utilitarios
  List,
  Grid3x3,
  Trash2,
  Image,

  // Filtros y detalles
  ChevronDown,
  Clock,
  Phone,
  MapPin,
  CheckCircle,

  // Maintenance / métricas
  ClipboardList,
  CalendarDays,
  Pencil,
  Calendar as CalendarIcon, // alias para evitar confusión, el name sigue siendo "calendar"
  Plus,
  Edit2,

  // Agregados requeridos por blockedaccounts
  Shield,
  Mail,
  CreditCard,
  Unlock,
  AlertTriangle,
  X
} from 'lucide-angular';

@NgModule({
  declarations: [
    Layout,
    Navbar,
    Sidebar,
    Dashboard,
    Employees,
    Reports,
    Analysis,
    Blockedaccounts,
    Companies,
    PropertiesAdmin,
    Quotes,
    Maintenance,
    Management,
    Requestadvisors,
    Testimonials,
    Configurations,
    Profile
  ],
  imports: [
    CommonModule,
    RouterModule,
    DashboardAdminRoutingModule,
    FormsModule,
    LucideAngularModule.pick({
      // Básicos / layout
      Home,
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
      Settings,
      Bell,
      Search,
      User,
      LogOut,
      TrendingUp,
      DollarSign,
      PieChart,
      FileDown,
      Loader2,
      Star,
      CheckCircle2,
      XCircle,
      Eye,

      // Properties / utilitarios
      List,
      Grid3x3,
      Trash2,
      Image,

      // Filtros y detalles
      ChevronDown,
      Clock,
      Phone,
      MapPin,
      CheckCircle,

      // Maintenance / Agenda
      ClipboardList,
      CalendarDays,
      Pencil,
      CalendarIcon,
      Plus,
      Edit2,

      // Requeridos por blockedaccounts
      Shield,         // estadísticas y tarjeta
      Mail,           // email
      CreditCard,     // DNI
      Unlock,         // botón desbloquear
      AlertTriangle,  // advertencia modal
      X               // cerrar modal
    })
  ],
  exports: [Dashboard]
})
export class DashboardAdminModule { }
