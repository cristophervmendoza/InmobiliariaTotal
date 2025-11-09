// core/guards/auth-can-activate.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authCanActivateGuard: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const roles = (route.data?.['roles'] ?? []) as string[];

  if (auth.isAuthenticated() && (roles.length === 0 || auth.hasRole(roles as any))) {
    return true;
  }
  return router.createUrlTree(['/auth/login'], { queryParams: { redirect: state.url } });
};
