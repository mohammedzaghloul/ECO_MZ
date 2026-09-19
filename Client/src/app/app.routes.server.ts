import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: 'shop/product/:id',
    renderMode: RenderMode.Client
  },
  {
    path: 'product/:id',
    renderMode: RenderMode.Client
  },
  {
    path: 'shop/product-details/:id',
    renderMode: RenderMode.Client
  },
  {
    path: 'product-details/:id',
    renderMode: RenderMode.Client
  },
  // Auth pages — rendered on client only (ToastrService needs DOM)
  {
    path: 'account/register',
    renderMode: RenderMode.Client
  },
  {
    path: 'account/login',
    renderMode: RenderMode.Client
  },
  {
    path: 'account/active',
    renderMode: RenderMode.Client
  },
  {
    path: 'account/forgot-password',
    renderMode: RenderMode.Client
  },
  {
    path: 'account/reset-password',
    renderMode: RenderMode.Client
  },
  // Admin — cookie-authenticated and guarded, so it must render on the
  // client: during SSR the guard has no auth cookie and bounces /admin
  // to the login page even for signed-in admins. The trailing '**' makes
  // the entry cover /admin and every child page below it.
  {
    path: 'admin/**',
    renderMode: RenderMode.Client
  },
  {
    path: '**',
    renderMode: RenderMode.Server
  }
];
