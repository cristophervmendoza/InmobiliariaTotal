import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, tap, Observable } from 'rxjs';
import { Router } from '@angular/router';
import { ApiService } from './api.service';

export type UserRole = 'admin' | 'agent' | 'client';

export interface LoginDto {
  email: string;
  password: string;
}

export interface AuthResponse {
  exito: boolean;
  mensaje?: string;
  usuario?: UserSession;
}

// ✅ Actualizada para coincidir con la respuesta de tu API
export interface UserSession {
  idUsuario: number;
  nombre: string;           // "Beba"
  email: string;            // "user@example.com"
  dni: string;              // "13123131"
  telefono: string;         // "960423875"
  nombreCorto: string;      // "Beba"
  iniciales: string;        // "BE"
  idEstadoUsuario: number;  // 1
  rol: UserRole;            // "admin"
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly KEY = 'auth_session';
  private api = inject(ApiService);
  private router = inject(Router);

  private currentSession$ = new BehaviorSubject<UserSession | null>(null);
  session$ = this.currentSession$.asObservable();

  constructor() {
    const cached = localStorage.getItem(this.KEY);
    if (cached) {
      try {
        const session = JSON.parse(cached);
        // ✅ Normalizar rol al cargar desde cache
        if (session?.rol) {
          session.rol = this.normalizeRole(session.rol);
        }
        this.currentSession$.next(session);
      } catch {
        localStorage.removeItem(this.KEY);
      }
    }
  }

  login(email: string, password: string): Observable<AuthResponse> {
    const dto: LoginDto = { email, password };
    return this.api.post<AuthResponse>('/api/Usuario/login', dto).pipe(
      tap(res => {
        if (res.exito && res.usuario) {
          // ✅ Normalizar rol a minúsculas
          res.usuario.rol = this.normalizeRole(res.usuario.rol);

          console.log('✅ Login exitoso:', res.usuario);

          this.currentSession$.next(res.usuario);
          localStorage.setItem(this.KEY, JSON.stringify(res.usuario));
        }
      })
    );
  }

  logout(): void {
    console.log('🚪 Cerrando sesión');
    this.currentSession$.next(null);
    localStorage.removeItem(this.KEY);
    this.router.navigate(['/auth/login']);
  }

  getSession(): UserSession | null {
    return this.currentSession$.value;
  }

  isAuthenticated(): boolean {
    return !!this.currentSession$.value;
  }

  hasRole(role: UserRole | UserRole[]): boolean {
    const session = this.getSession();
    if (!session) return false;
    if (Array.isArray(role)) return role.includes(session.rol);
    return session.rol === role;
  }

  get user(): UserSession | null {
    return this.currentSession$.value;
  }

  get role(): UserRole | null {
    return this.currentSession$.value?.rol ?? null;
  }

  // ✅ Helper para normalizar roles
  private normalizeRole(rol: any): UserRole {
    if (typeof rol === 'string') {
      const normalized = rol.toLowerCase().trim();
      if (normalized === 'admin' || normalized === 'administrador') return 'admin';
      if (normalized === 'agent' || normalized === 'agente' || normalized === 'agenteinmobiliario') return 'agent';
      if (normalized === 'client' || normalized === 'cliente') return 'client';
    }
    return rol;
  }
}
