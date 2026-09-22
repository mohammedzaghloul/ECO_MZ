import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Register } from './register/register';
import { Login } from './login/login';
import { Active } from './active/active';
import { ForgotPassword } from './forgot-password/forgot-password';
import { ResetPasswordPage } from './reset-password/reset-password';

const routes: Routes = [
  { path: 'register', component: Register },
  { path: 'login', component: Login },
  { path: 'active', component: Active },
  { path: 'forgot-password', component: ForgotPassword },
  { path: 'reset-password', component: ResetPasswordPage },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class IdentityRoutingModule {}
