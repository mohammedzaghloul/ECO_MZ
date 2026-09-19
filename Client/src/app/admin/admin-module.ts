import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { Admin } from './admin';
import { adminGuard } from './admin.guard';
import { AdminDashboard } from './dashboard/dashboard';
import { AdminProducts } from './products/products';
import { AdminCategories } from './categories/categories';
import { AdminOrders } from './orders/orders';
import { AdminLandingList } from './landing/landing-list';
import { AdminLandingEditor } from './landing/landing-editor';
import { AdminCustomers } from './customers/customers';
import { AdminDiscounts } from './discounts/discounts';
import { AdminLocations } from './locations/locations';
import { AdminSettings } from './settings/settings';
import { AdminEmailSettings } from './email-settings/email-settings';
import { SharedModule } from '../shared/shared-module';

const routes: Routes = [
  {
    path: '',
    component: Admin,
    canActivate: [adminGuard],
    children: [
      { path: '', component: AdminDashboard },
      { path: 'products', component: AdminProducts },
      { path: 'categories', component: AdminCategories },
      { path: 'orders', component: AdminOrders },
      { path: 'customers', component: AdminCustomers },
      { path: 'coupons', component: AdminDiscounts },
      { path: 'discounts', redirectTo: 'coupons', pathMatch: 'full' },
      { path: 'locations', component: AdminLocations },
      { path: 'landing', component: AdminLandingList },
      { path: 'settings', component: AdminSettings },
      { path: 'email-settings', component: AdminEmailSettings },
      { path: 'landing/edit/:id', component: AdminLandingEditor }
    ]
  }
];

@NgModule({
  declarations: [Admin, AdminDashboard, AdminProducts, AdminCategories, AdminOrders, AdminLandingList, AdminLandingEditor, AdminCustomers, AdminDiscounts, AdminLocations, AdminSettings, AdminEmailSettings],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, RouterModule.forChild(routes)]
})
export class AdminModule {}
