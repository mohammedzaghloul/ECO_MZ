import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Shop } from './shop';
import { RouterModule } from '@angular/router';
import { ShopItem } from './shop-item/shop-item';
import { ProductDetails } from './product-details/product-details';
import { ShopRoutingModule } from './shop-routing-module';

@NgModule({
  declarations: [Shop, ShopItem, ProductDetails],
  imports: [CommonModule, FormsModule, RouterModule, ShopRoutingModule],
})
export class ShopModule {}
