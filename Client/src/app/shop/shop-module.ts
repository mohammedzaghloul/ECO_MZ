import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Shop } from './shop';
import { RouterModule } from '@angular/router';
import { ProductDetails } from './product-details/product-details';
import { SharedModule } from '../shared/shared-module';
import { ShopRoutingModule } from './shop-routing-module';

@NgModule({
  declarations: [Shop, ProductDetails],
  imports: [CommonModule, FormsModule, RouterModule, ShopRoutingModule, SharedModule],
})
export class ShopModule {}
