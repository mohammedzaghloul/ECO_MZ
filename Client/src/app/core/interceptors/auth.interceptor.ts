import { HttpContextToken, HttpInterceptorFn } from '@angular/common/http';
import { catchError, finalize, shareReplay, switchMap, throwError, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';

export const SKIP_AUTH_REFRESH = new HttpContextToken<boolean>(() => false);
let refreshInFlight$: Observable<unknown> | null = null;

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const http = inject(HttpClient);
  const shouldSkip =
    request.context.get(SKIP_AUTH_REFRESH) ||
    /\/Account\/(Login|register|refresh|Logout|google-)/i.test(request.url);

  return next(request.clone({ withCredentials: true })).pipe(
    catchError((error) => {
      if (error.status !== 401 || shouldSkip) {
        return throwError(() => error);
      }

      refreshInFlight$ ??= http.post(
        '/api/Account/refresh',
        {},
        {
          withCredentials: true,
          context: request.context.set(SKIP_AUTH_REFRESH, true),
        }
      ).pipe(
        finalize(() => refreshInFlight$ = null),
        shareReplay({ bufferSize: 1, refCount: false })
      );

      return refreshInFlight$.pipe(
        switchMap(() => next(request.clone({ withCredentials: true }))),
        catchError(() => throwError(() => error))
      );
    })
  );
};
