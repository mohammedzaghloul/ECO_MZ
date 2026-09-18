import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProductDetails } from './product-details/product-details';
import { Shop } from './shop';

const routes: Routes = [
  { path: '', component: Shop },
  { path: 'product/:id', component: ProductDetails },
  { path: 'product-details/:id', component: ProductDetails },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ShopRoutingModule {}
