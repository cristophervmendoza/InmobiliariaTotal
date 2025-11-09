import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

interface MenuItem { name: string; path: string; icon: string; }
interface MenuSection { title: string; items: MenuItem[]; }

@Component({
  selector: 'app-sidebar-agent',

  standalone: false,
  templateUrl: './sidebar-agent.html',
  styleUrl: './sidebar-agent.css',
})
export class SidebarAgent {


  constructor(private auth: AuthService, private router: Router) { }

  menuSections: MenuSection[] = [
    {
      title: 'PRINCIPAL',
      items: [
        { name: 'Dashboard', path: '/agent/dashboard', icon: 'home' },
        { name: 'Reportes', path: '/agent/reports', icon: 'file-text' }
      ]
    },
    {
      title: 'OPERACIONES',
      items: [
        { name: 'Gestión', path: '/agent/catalog', icon: 'briefcase' },
        { name: 'Propiedades', path: '/agent/properties', icon: 'building-2' },
        { name: 'Citas', path: '/agent/messages', icon: 'calendar' },
        { name: 'Mantenimiento', path: '/agent/maintenance', icon: 'wrench' },
        { name: 'Testimonios', path: '/agent/agenda', icon: 'message-square' }
      ]
    },
  ];

  logout(): void {
    this.auth.logout();                  // limpia la sesión (localStorage, etc.)
    this.router.navigate(['/auth/login']); // navega al login
  }
}
