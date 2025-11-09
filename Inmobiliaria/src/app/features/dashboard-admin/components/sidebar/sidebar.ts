import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

interface MenuItem { name: string; path: string; icon: string; }
interface MenuSection { title: string; items: MenuItem[]; }

@Component({
  selector: 'app-sidebar',
  standalone: false,
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  constructor(private auth: AuthService, private router: Router) { }

  menuSections: MenuSection[] = [
    {
      title: 'PRINCIPAL',
      items: [
        { name: 'Dashboard', path: '/admin/dashboard', icon: 'home' },
        { name: 'Reportes', path: '/admin/reports', icon: 'file-text' },
        { name: 'Análisis', path: '/admin/analysis', icon: 'bar-chart-3' }
      ]
    },
    {
      title: 'OPERACIONES',
      items: [
        { name: 'Gestión', path: '/admin/management', icon: 'briefcase' },
        { name: 'Propiedades', path: '/admin/properties', icon: 'building-2' },
        { name: 'Citas', path: '/admin/quotes', icon: 'calendar' },
        { name: 'Solicitud Asesores', path: '/admin/requestadvisors', icon: 'users' },
        { name: 'Mantenimiento', path: '/admin/maintenance', icon: 'wrench' },
        { name: 'Testimonios', path: '/admin/testimonials', icon: 'message-square' }
      ]
    },
    {
      title: 'ADMINISTRACIÓN',
      items: [
        { name: 'Empleados', path: '/admin/employees', icon: 'id-card' },
        { name: 'Empresas', path: '/admin/companies', icon: 'briefcase-business' },
        { name: 'Cuentas Bloqueadas', path: '/admin/blockedaccounts', icon: 'lock' }
      ]
    }
  ];

  logout(): void {
    this.auth.logout();                  // limpia la sesión (localStorage, etc.)
    this.router.navigate(['/auth/login']); // navega al login
  }
}
