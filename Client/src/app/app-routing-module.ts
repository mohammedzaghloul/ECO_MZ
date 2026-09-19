import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AccountPage } from './account-page';
import { AboutPage } from './about-page';
import { BestSellersPage } from './best-sellers-page';
import { HomePage } from './home-page';
import { OrderDetailPage } from './order-detail-page';
import { RegisterStepperComponent } from './identity/register-stepper/register-stepper.component';
import { WishlistPage } from './wishlist-page';
import { LandingViewPage } from './landing-view-page';
import { LandingDemoPage } from './landing-demo-page/landing-demo-page';

const routes: Routes = [
  { path: '', component: HomePage },
  { path: 'shop', loadChildren: () => import('./shop/shop-module').then((m) => m.ShopModule) },
  { path: 'basket', loadChildren: () => import('./basket/basket-module').then((m) => m.BasketModule) },
  { path: 'account', loadChildren: () => import('./identity/identity-module').then((m) => m.IdentityModule) },
  { path: 'checkout', loadChildren: () => import('./checkout/checkout-module').then((m) => m.CheckoutModule) },
  { path: 'admin', loadChildren: () => import('./admin/admin-module').then((m) => m.AdminModule) },
  { path: 'login',    redirectTo: 'account/login',    pathMatch: 'full' },
  { path: 'register-stepper', component: RegisterStepperComponent },
  { path: 'register', redirectTo: 'account/register', pathMatch: 'full' },
  { path: 'my-order',     component: AccountPage,     data: { page: 'orders' } },
  { path: 'my-order/:id', component: OrderDetailPage },
  { path: 'profile',  component: AccountPage, data: { page: 'profile' } },
  { path: 'about',    component: AboutPage },
  { path: 'best-sellers', component: BestSellersPage },
  { path: 'wishlist', component: WishlistPage },
  { path: 'landing/demo', component: LandingDemoPage },
  { path: 'landing/:slug', component: LandingViewPage },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, {
      anchorScrolling: 'enabled',
      scrollPositionRestoration: 'enabled',
    }),
  ],
  exports: [RouterModule]
})
export class AppRoutingModule { }
