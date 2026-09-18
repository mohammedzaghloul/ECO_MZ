import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { CoreModule } from './core/core-module';
import { SharedModule } from './shared/shared-module';
import { ShopModule } from './shop/shop-module';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { AccountPage } from './account-page';
import { AboutPage } from './about-page';
import { BestSellersPage } from './best-sellers-page';
import { HomePage } from './home-page';

@NgModule({
  declarations: [
    App,
    AccountPage,
    AboutPage,
    BestSellersPage,
    HomePage
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    CoreModule,
    SharedModule,
    ShopModule,
    FormsModule
],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withFetch())
  ],
  bootstrap: [App]
})
export class AppModule { }
