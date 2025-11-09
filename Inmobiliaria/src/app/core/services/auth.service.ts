import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

export type UserRole = 'admin' | 'agent' | 'client';

export type UserRecord = {
  email: string;
  password: string;
  role: UserRole;
  name: string;
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly KEY = 'auth_session';

  // Usuarios de ejemplo
  private users: UserRecord[] = [
    { email: 'admin@idealhome.pe', password: 'Admin#123', role: 'admin', name: 'Administrador' },
    { email: 'agent@idealhome.pe', password: 'Agent#123', role: 'agent', name: 'Agente' },
    { email: 'client@idealhome.pe', password: 'Client#123', role: 'client', name: 'Cliente' },
  ];

  constructor(private router: Router) { }

  login(email: string, password: string) {
    const found = this.users.find(u => u.email === email && u.password === password);
    if (!found) return null;

    const session = {
      email: found.email,
      role: found.role,
      name: found.name,
      token: this.generateToken(found.email, found.role)
    };
    localStorage.setItem(this.KEY, JSON.stringify(session));
    return session;
  }

  logout() {
    localStorage.removeItem(this.KEY);
    this.router.navigate(['/login']);
  }

  getSession(): { email: string; role: UserRole; name: string; token: string } | null {
    const raw = localStorage.getItem(this.KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  isAuthenticated(): boolean {
    return !!this.getSession();
  }

  hasRole(role: UserRole | UserRole[]): boolean {
    const s = this.getSession();
    if (!s) return false;
    if (Array.isArray(role)) return role.includes(s.role);
    return s.role === role;
  }

  private generateToken(email: string, role: UserRole) {
    // token de demostración
    return btoa(`${email}|${role}|${Date.now()}`);
  }
}
