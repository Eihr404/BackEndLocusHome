import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const allowedRoles = (route.data['roles'] as string[] | undefined) ?? [];

  if (!allowedRoles.length || allowedRoles.includes(authService.role() ?? '')) {
    return true;
  }

  const fallback = authService.role() === 'socio' ? '/socio' : '/';
  return router.createUrlTree([fallback]);
};
