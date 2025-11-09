// core/guards/auth-can-load.guard.ts
import { inject } from '@angular/core';
import { CanLoadFn, Route, UrlSegment, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authCanLoadGuard: CanLoadFn = (route: Route, segments: UrlSegment[]) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const roles = (route.data?.['roles'] ?? []) as string[];
  const url = '/' + segments.map(s => s.path).join('/');

  if (auth.isAuthenticated() && (roles.length === 0 || auth.hasRole(roles as any))) {
    return true;
  }
  return router.createUrlTree(['/auth/login'], { queryParams: { redirect: url } });
};
