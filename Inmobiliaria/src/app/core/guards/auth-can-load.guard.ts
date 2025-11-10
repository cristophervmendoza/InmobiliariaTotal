import { inject } from '@angular/core';
import { CanLoadFn, Route, UrlSegment, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authCanLoadGuard: CanLoadFn = (route: Route, segments: UrlSegment[]) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const roles = (route.data?.['roles'] ?? []) as string[];
  const url = '/' + segments.map(s => s.path).join('/');

  const isAuth = auth.isAuthenticated();
  const role = auth.role?.toLowerCase() ?? null;

  console.log('🛡️ CanLoad - URL:', url);
  console.log('🛡️ CanLoad - Rol usuario:', role);

  // ❌ No autenticado
  if (!isAuth) {
    return router.createUrlTree(['/auth/login'], { queryParams: { redirect: url } });
  }

  // ✅ Sin restricción de rol
  if (roles.length === 0) {
    return true;
  }

  // ✅ Rol autorizado
  if (role && roles.map(r => r.toLowerCase()).includes(role)) {
    return true;
  }

  // ❌ Rol no autorizado - redirige a su dashboard
  const dashboardPath = getDashboardByRole(role);
  return router.createUrlTree([dashboardPath]);
};

function getDashboardByRole(role: string | null): string {
  switch (role) {
    case 'admin':
      return '/admin/dashboard';
    case 'agent':
      return '/agent/dashboard';
    case 'client':
      return '/client/dashboard';
    default:
      return '/auth/login';
  }
}
