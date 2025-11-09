// src/app/core/guards/role-can-activate-child.guard.ts
import { inject } from '@angular/core';
import { CanActivateChildFn, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service'; // ajusta la ruta si tu servicio está en core/services

export const roleCanActivateChildGuard: CanActivateChildFn =
  (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
    const auth = inject(AuthService);
    const router = inject(Router);

    const allowed = (route.parent?.data?.['roles'] ?? route.data?.['roles'] ?? []) as string[];

    if (auth.isAuthenticated() && (allowed.length === 0 || auth.hasRole(allowed as any))) {
      return true;
    }
    return router.createUrlTree(['/auth/login'], { queryParams: { redirect: state.url } });
  };
