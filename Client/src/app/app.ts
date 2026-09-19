import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { BasketService } from './basket/basket.service';
import { StoreSettingsService } from './core/Services/store-settings.service';
import { OfflineSupportService } from './core/Services/offline-support.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.scss'
})
export class App implements OnInit {
  isAuthPage = false;
  isAdminPage = false;
  isLandingPage = false;

  constructor(
    private basketService: BasketService,
    private storeSettings: StoreSettingsService,
    private offlineSupport: OfflineSupportService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.basketService.load();
    this.storeSettings.load();
    this.checkLayoutRoute(this.router.url);

    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd)
    ).subscribe((event) => {
      this.checkLayoutRoute(event.urlAfterRedirects || event.url);
    });
  }

  /* Storefront chrome (header, footer, bottom nav) is hidden on auth, admin,
     and dedicated full-screen landing pages. */
  private checkLayoutRoute(url: string): void {
    const cleanUrl = url.split('?')[0].split('#')[0];
    this.isAuthPage = cleanUrl.startsWith('/account') || cleanUrl === '/login' || cleanUrl === '/register';
    this.isAdminPage = cleanUrl === '/admin' || cleanUrl.startsWith('/admin/');
    this.isLandingPage = cleanUrl.startsWith('/landing') || cleanUrl.startsWith('/lp/');
  }
}