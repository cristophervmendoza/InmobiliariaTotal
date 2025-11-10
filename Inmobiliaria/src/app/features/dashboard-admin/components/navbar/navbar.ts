import { Component, ElementRef, HostListener, ViewChild, OnInit, inject, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, UserSession } from '../../../../core/services/auth.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class Navbar implements OnInit, OnDestroy {
  private auth = inject(AuthService);
  private router = inject(Router);
  private sessionSub?: Subscription;

  showDropdown = false;
  userName = 'Usuario';
  userRole = 'Rol no definido';
  userEmail = '';
  userInitials = 'U';

  @ViewChild('userMenuRoot') userMenuRoot!: ElementRef<HTMLElement>;

  ngOnInit(): void {
    this.loadUserData();

    this.sessionSub = this.auth.session$.subscribe(session => {
      if (session) {
        this.updateUserData(session);
      } else {
        this.resetUserData();
      }
    });
  }

  ngOnDestroy(): void {
    this.sessionSub?.unsubscribe();
  }

  private loadUserData(): void {
    const session = this.auth.getSession();
    if (session) {
      this.updateUserData(session);
    }
  }

  private updateUserData(session: UserSession): void {
    // ✅ Usar los campos que vienen del API
    this.userName = session.nombre || session.nombreCorto || 'Usuario';
    this.userEmail = session.email || '';
    this.userInitials = session.iniciales || this.calculateInitials(this.userName);

    // ✅ Traducir rol a español
    switch (session.rol) {
      case 'admin':
        this.userRole = 'Administrador';
        break;
      case 'agent':
        this.userRole = 'Agente Inmobiliario';
        break;
      case 'client':
        this.userRole = 'Cliente';
        break;
      default:
        this.userRole = session.rol || 'Usuario';
    }

    console.log('👤 Usuario cargado:', {
      nombre: this.userName,
      email: this.userEmail,
      rol: this.userRole,
      iniciales: this.userInitials
    });
  }

  private resetUserData(): void {
    this.userName = 'Usuario';
    this.userRole = 'Rol no definido';
    this.userEmail = '';
    this.userInitials = 'U';
  }

  private calculateInitials(name: string): string {
    if (!name) return 'U';
    return name
      .split(' ')
      .filter(Boolean)
      .map(n => n[0]!)
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  get initials(): string {
    return this.userInitials;
  }

  toggleDropdown(ev?: MouseEvent): void {
    if (ev) ev.stopPropagation();
    this.showDropdown = !this.showDropdown;
  }

  logout(): void {
    this.showDropdown = false;
    this.auth.logout();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.showDropdown) return;
    const root = this.userMenuRoot?.nativeElement;
    const target = event.target as Node | null;
    if (!root || !target) return;
    if (!root.contains(target)) {
      this.showDropdown = false;
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.showDropdown) this.showDropdown = false;
  }
}
