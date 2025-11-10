// features/dashboard-admin/dashboard-admin.module.ts

import { FormsModule } from '@angular/forms';

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { DashboardAgentRoutingModule } from './dashboard-agent-routing.module';

// Layout
import { Layout } from './layout/layout';

// UI propias
import { NavbarAgent } from './components/navbar-agent/navbar-agent';
import { SidebarAgent } from './components/sidebar-agent/sidebar-agent';

// Pages

import { AgendaAgent } from './pages/agenda-agent/agenda-agent';
import { CatalogAgent } from './pages/catalog-agent/catalog-agent';
import { CompaniesAgent } from './pages/companies-agent/companies-agent';
import { DashboardAgent } from './pages/dashboard-agent/dashboard-agent';
import { MaintenanceAgent } from './pages/maintenance-agent/maintenance-agent';
import { MessagesAgent } from './pages/messages-agent/messages-agent';
import { MisPropiedadesComponent } from './pages/properties-agent/mis-propiedades.component';
import { DetallePropiedadAgenteComponent } from './pages/properties-agent/detalle-propiedad-agente.component';




import { ReportsAgent } from './pages/reports-agent/reports-agent';

// Extras
import { ConfigurationsAgent } from './pages/configurations-agent/configurations-agent';
import { ProfileAgent } from './pages/profile-agent/profile-agent';


// Iconos (lucide-angular)
import {
  LucideAngularModule,
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
  LogOut
} from 'lucide-angular';

@NgModule({
  declarations: [
    Layout,
    NavbarAgent,
    SidebarAgent,


    AgendaAgent,
    ReportsAgent,


    CompaniesAgent,

    MessagesAgent,
    MaintenanceAgent,
    ConfigurationsAgent,
    ProfileAgent
  ],
  imports: [
    CommonModule,
    RouterModule,
    DashboardAgentRoutingModule,
    DashboardAgent,
    MisPropiedadesComponent,
    DetallePropiedadAgenteComponent,
    CatalogAgent,
    FormsModule,
    LucideAngularModule.pick({
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
      LogOut
    })
  ]
})
export class DashboardAgentModule {

}
