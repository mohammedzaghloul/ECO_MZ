import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { BasketRoutingModule } from './basket-routing-module';
import { Basket } from './basket/basket';
import { SharedModule } from '../shared/shared-module';

@NgModule({
  declarations: [Basket],
  imports: [CommonModule, RouterModule, BasketRoutingModule, SharedModule],
})
export class BasketModule {}
