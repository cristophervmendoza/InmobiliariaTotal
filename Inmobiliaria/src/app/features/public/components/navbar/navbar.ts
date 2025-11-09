import { Component, ElementRef, HostListener, ViewChild } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class Navbar {



  isMenuOpen = false;
  isUserMenuOpen = false;

  @ViewChild('userMenuRoot') userMenuRoot!: ElementRef<HTMLElement>;

  navigationItems = [
    { name: 'Inicio', path: '/home' },
    { name: 'Se un asesor', path: '/beasesor' },
    { name: 'Testimonios', path: '/testimony' },
    { name: 'Ofrecer tu Inmueble', path: '/offer' },
    { name: 'Propiedades', path: '/properties' },
  ];

  constructor(private router: Router) {
    // Cierra menús al cambiar de ruta
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => {
        this.isMenuOpen = false;
        this.isUserMenuOpen = false;
      });
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
  }

  toggleUserMenu(ev?: MouseEvent): void {
    if (ev) ev.stopPropagation(); // evita que el click burbujee al document
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  // Cerrar user menu con clic fuera
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.isUserMenuOpen) return;
    const root = this.userMenuRoot?.nativeElement;
    const target = event.target as Node | null;
    if (!root || !target) return;
    if (!root.contains(target)) {
      this.isUserMenuOpen = false;
    }
  }

  // Cerrar con Escape
  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isUserMenuOpen) this.isUserMenuOpen = false;
  }
}
