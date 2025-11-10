import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { LoginService } from '../services/login.service';

export const authCanActivateGuard: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const auth = inject(LoginService);
  const router = inject(Router);
  const roles = (route.data?.['roles'] ?? []) as string[];

  const isAuth = auth.isAuthenticated;
  const role = auth.role?.toLowerCase() ?? null;

  if (isAuth && (roles.length === 0 || (role && roles.map(r => r.toLowerCase()).includes(role)))) {
    return true;
  }
  return router.createUrlTree(['/auth/login'], { queryParams: { redirect: state.url } });
};
