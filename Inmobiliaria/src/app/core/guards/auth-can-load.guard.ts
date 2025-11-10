import { inject } from '@angular/core';
import { CanLoadFn, Route, UrlSegment, Router } from '@angular/router';
import { LoginService } from '../services/login.service';

export const authCanLoadGuard: CanLoadFn = (route: Route, segments: UrlSegment[]) => {
  const auth = inject(LoginService);
  const router = inject(Router);
  const roles = (route.data?.['roles'] ?? []) as string[];
  const url = '/' + segments.map(s => s.path).join('/');

  const isAuth = auth.isAuthenticated;
  const role = auth.role?.toLowerCase() ?? null;

  if (isAuth && (roles.length === 0 || (role && roles.map(r => r.toLowerCase()).includes(role)))) {
    return true;
  }
  return router.createUrlTree(['/auth/login'], { queryParams: { redirect: url } });
};
