import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ToastrModule } from 'ngx-toastr';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { CoreModule } from './core/core-module';
import { SharedModule } from './shared/shared-module';
import { ShopModule } from './shop/shop-module';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideServiceWorker } from '@angular/service-worker';
import { isDevMode } from '@angular/core';
import { NgxSpinnerModule } from 'ngx-spinner';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { AccountPage } from './account-page';
import { AboutPage } from './about-page';
import { BestSellersPage } from './best-sellers-page';
import { HomePage } from './home-page';
import { OrderDetailPage } from './order-detail-page';
import { RegisterStepperComponent } from './identity/register-stepper/register-stepper.component';
import { WishlistPage } from './wishlist-page';
import { LandingViewPage } from './landing-view-page';
import { LandingDemoPage } from './landing-demo-page/landing-demo-page';

@NgModule({
  declarations: [
    App,
    AccountPage,
    AboutPage,
    BestSellersPage,
    HomePage,
    OrderDetailPage,
    RegisterStepperComponent,
    WishlistPage,
    LandingViewPage,
    LandingDemoPage
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    CoreModule,
    SharedModule,
    ShopModule,
    FormsModule,
    ReactiveFormsModule,
    ToastrModule.forRoot({
      positionClass: 'toast-top-right',
      timeOut: 4000,
      closeButton: true,
      progressBar: true,
    }),
    NgxSpinnerModule,
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withFetch(), withInterceptors([loadingInterceptor, authInterceptor])),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000',
    }),
  ],
  bootstrap: [App]
})
export class AppModule { }
