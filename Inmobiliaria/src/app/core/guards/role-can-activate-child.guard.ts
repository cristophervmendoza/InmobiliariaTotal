import { inject } from '@angular/core';
import { CanActivateChildFn, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleCanActivateChildGuard: CanActivateChildFn =
  (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
    const auth = inject(AuthService);
    const router = inject(Router);

    const allowed = (route.parent?.data?.['roles'] ?? route.data?.['roles'] ?? []) as string[];
    const isAuth = auth.isAuthenticated();
    const role = auth.role?.toLowerCase() ?? null;

    console.log('🛡️ CanActivateChild - Ruta:', state.url);
    console.log('🛡️ CanActivateChild - Rol usuario:', role);

    // ❌ No autenticado
    if (!isAuth) {
      return router.createUrlTree(['/auth/login'], { queryParams: { redirect: state.url } });
    }

    // ✅ Sin restricción o rol autorizado
    if (allowed.length === 0 || (role && allowed.map(r => r.toLowerCase()).includes(role))) {
      return true;
    }

    // ❌ Rol no autorizado
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
