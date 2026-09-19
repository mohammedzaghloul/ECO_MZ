import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { CanActivateFn } from '@angular/router';
import { of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AccountService } from '../core/Services/account.service';

/**
 * Blocks /admin for anyone who is not signed in with the Admin role.
 * The role travels in the JWT (ClaimTypes.Role) and is mirrored on
 * UserDto.roles by /Account/GetCurrentUser.
 */
export const adminGuard: CanActivateFn = () => {
  const account = inject(AccountService);
  const router = inject(Router);
  const hasAdminRole = (roles: string[] | undefined): boolean =>
    roles?.some((role) => role.trim().toLowerCase() === 'admin') ?? false;

  if (hasAdminRole(account.currentUser()?.roles)) {
    return true;
  }

  return account.getCurrentUser().pipe(
    map((response) => {
      const roles = response?.data?.roles ?? [];
      if (hasAdminRole(roles)) {
        account.currentUser.set(response.data);
        return true;
      }

      return router.parseUrl('/');
    }),
    catchError((error: { status?: number }) => {
      if (error?.status === 401 || error?.status === 403) {
        return of(router.createUrlTree(['/account/login'], {
          queryParams: { returnUrl: router.url }
        }));
      }

      return of(router.parseUrl('/'));
    })
  );
};
