import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authCanActivateGuard: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const roles = (route.data?.['roles'] ?? []) as string[];

  const isAuth = auth.isAuthenticated();
  const role = auth.role?.toLowerCase() ?? null;

  console.log('🛡️ Guard - Ruta:', state.url);
  console.log('🛡️ Guard - Roles permitidos:', roles);
  console.log('🛡️ Guard - Rol usuario:', role);

  // ❌ No autenticado - redirige al login
  if (!isAuth) {
    console.log('❌ No autenticado, redirigiendo a login');
    return router.createUrlTree(['/auth/login'], { queryParams: { redirect: state.url } });
  }

  // ✅ Ruta sin restricción de rol
  if (roles.length === 0) {
    console.log('✅ Ruta sin restricción, acceso permitido');
    return true;
  }

  // ✅ Rol coincide con los permitidos
  if (role && roles.map(r => r.toLowerCase()).includes(role)) {
    console.log('✅ Rol autorizado, acceso permitido');
    return true;
  }

  // ❌ Rol no autorizado - redirige a SU dashboard
  console.log('❌ Rol no autorizado, redirigiendo a su dashboard');
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
