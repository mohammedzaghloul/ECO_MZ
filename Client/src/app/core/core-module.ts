import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Navbar } from './navbar/navbar';
import { Footer } from './footer/footer';
import { AppRoutingModule } from "../app-routing-module";
import { RouterModule } from '@angular/router';

@NgModule({
  declarations: [Navbar, Footer],
  imports: [CommonModule, FormsModule, AppRoutingModule,RouterModule],
  exports: [Navbar, Footer]
})
export class CoreModule {}
