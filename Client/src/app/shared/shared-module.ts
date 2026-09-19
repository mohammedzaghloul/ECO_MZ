import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HeaderComponent } from './components/header/header.component';
import { BottomNavComponent } from './components/bottom-nav/bottom-nav.component';
import { ShopItem } from '../shop/shop-item/shop-item';
import { OrderTrackingComponent } from './components/order-tracking/order-tracking.component';
import { StepperComponent } from './components/stepper/stepper.component';
import { TranslatePipe } from './pipes/translate.pipe';
import { TranslateValuePipe } from './pipes/translate-value.pipe';
import { StatusBadgeComponent } from './components/status-badge/status-badge';
import { StatCardComponent } from './components/stat-card/stat-card';
import { StoreCurrencyPipe } from './pipes/store-currency.pipe';

@NgModule({
  declarations: [
    HeaderComponent,
    BottomNavComponent,
    ShopItem,
    OrderTrackingComponent,
    StepperComponent,
    TranslatePipe,
    TranslateValuePipe,
    StatusBadgeComponent,
    StatCardComponent,
    StoreCurrencyPipe
  ],
  imports: [CommonModule, FormsModule, RouterModule],
  exports: [
    HeaderComponent,
    BottomNavComponent,
    FormsModule,
    ShopItem,
    OrderTrackingComponent,
    StepperComponent,
    TranslatePipe,
    TranslateValuePipe,
    StatusBadgeComponent,
    StatCardComponent,
    StoreCurrencyPipe
  ]
})
export class SharedModule {}
