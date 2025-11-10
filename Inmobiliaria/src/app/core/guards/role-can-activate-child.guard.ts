import { inject } from '@angular/core';
import { CanActivateChildFn, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { LoginService } from '../services/login.service';

export const roleCanActivateChildGuard: CanActivateChildFn =
  (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
    const auth = inject(LoginService);
    const router = inject(Router);

    const allowed = (route.parent?.data?.['roles'] ?? route.data?.['roles'] ?? []) as string[];
    const isAuth = auth.isAuthenticated;
    const role = auth.role?.toLowerCase() ?? null;

    if (isAuth && (allowed.length === 0 || (role && allowed.map(r => r.toLowerCase()).includes(role)))) {
      return true;
    }
    return router.createUrlTree(['/auth/login'], { queryParams: { redirect: state.url } });
  };

