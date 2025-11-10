import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const loginRedirectGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const isAuth = auth.isAuthenticated();
  const role = auth.role?.toLowerCase();

  // ✅ Si ya está autenticado, redirige a su dashboard
  if (isAuth && role) {
    console.log('✅ Usuario ya autenticado, redirigiendo a dashboard:', role);

    const dashboardPath = getDashboardByRole(role);
    router.navigate([dashboardPath]);
    return false; // Bloquea el acceso al login
  }

  // ✅ Si no está autenticado, permite acceso al login
  console.log('✅ Usuario no autenticado, permitiendo acceso a login');
  return true;
};

// Helper: Obtiene la ruta del dashboard según el rol
function getDashboardByRole(role: string): string {
  switch (role) {
    case 'admin':
      return '/admin/dashboard';
    case 'agent':
      return '/agent/dashboard';
    case 'client':
      return '/client/dashboard';
    default:
      return '/';
  }
}
