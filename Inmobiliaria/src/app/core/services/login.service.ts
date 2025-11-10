import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, tap } from 'rxjs';
import { ApiService } from './api.service';
import { LoginDto, LoginResponse, LoginUser } from '../models/modelauth';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class LoginService {
  private api = inject(ApiService);
  private router = inject(Router);

  private currentUser$ = new BehaviorSubject<LoginUser | null>(null);
  user$ = this.currentUser$.asObservable();

  constructor() {
    const cached = localStorage.getItem('user');
    if (cached) this.currentUser$.next(JSON.parse(cached));
  }

  login(dto: LoginDto) {
    return this.api.post<LoginResponse>('/api/Usuario/login', dto).pipe(
      tap(res => {
        if (res.exito && res.usuario) {
          // normalizar rol
          res.usuario.rol = (res.usuario.rol as any)?.toLowerCase?.() ?? res.usuario.rol;
          this.currentUser$.next(res.usuario);
          localStorage.setItem('user', JSON.stringify(res.usuario));
        }
      })
    );
  }


  logout() {
    this.currentUser$.next(null);
    localStorage.removeItem('user');
    this.router.navigate(['/auth/login']);
  }

  get user(): LoginUser | null { return this.currentUser$.value; }
  get role(): LoginUser['rol'] | null { return this.currentUser$.value?.rol ?? null; }
  get isAuthenticated(): boolean { return !!this.currentUser$.value; }
}
