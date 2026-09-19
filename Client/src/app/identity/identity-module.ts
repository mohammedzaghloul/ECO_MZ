import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { IdentityRoutingModule } from './identity-routing-module';
import { Register } from './register/register';
import { Login } from './login/login';
import { ReactiveFormsModule } from '@angular/forms';
import { Active } from './active/active';
import { ForgotPassword } from './forgot-password/forgot-password';
import { SharedModule } from '../shared/shared-module';

@NgModule({
  declarations: [Register, Login, Active, ForgotPassword],
  imports: [CommonModule, RouterModule, IdentityRoutingModule, ReactiveFormsModule, SharedModule],
})
export class IdentityModule {}
