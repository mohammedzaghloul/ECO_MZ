import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AccountPage } from './account-page';
import { AboutPage } from './about-page';
import { BestSellersPage } from './best-sellers-page';
import { HomePage } from './home-page';

const routes: Routes = [
  { path: '', component: HomePage },
  { path: 'shop', loadChildren: () => import('./shop/shop-module').then((module) => module.ShopModule) },
  { path: 'login', component: AccountPage, data: { page: 'login' } },
  { path: 'register', component: AccountPage, data: { page: 'register' } },
  { path: 'my-order', component: AccountPage, data: { page: 'orders' } },
  { path: 'about', component: AboutPage },
  { path: 'best-sellers', component: BestSellersPage },
  { path: '**', redirectTo:'',pathMatch: 'full' }
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
