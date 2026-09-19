import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { CheckoutComponent } from './checkout/checkout.component';
import { SharedModule } from '../shared/shared-module';

const routes: Routes = [
  { path: '', component: CheckoutComponent }
];

@NgModule({
  declarations: [CheckoutComponent],
  imports: [CommonModule, ReactiveFormsModule, RouterModule.forChild(routes), SharedModule]
})
export class CheckoutModule {}
