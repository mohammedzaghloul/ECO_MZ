import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AccountService } from '../core/Services/account.service';
import { LanguageService } from '../core/Services/language.service';
import { DashboardService } from '../core/Services/dashboard.service';

@Component({
  selector: 'app-admin',
  standalone: false,
  templateUrl: './admin.html',
  styleUrl: './admin.scss'
})
export class Admin {
  private readonly account = inject(AccountService);
  private readonly dashboardService = inject(DashboardService);
  private readonly router = inject(Router);
  readonly language = inject(LanguageService);

  /** AccountService keeps the shared auth state; the header already fills it. */
  readonly user = this.account.currentUser;
  menuOpen = false;

  /** Products nav badge: how many items are at or below the low-stock threshold. */
  readonly lowStockCount = signal(0);

  constructor() {
    // Store chrome that normally loads the user is hidden inside the admin
    // area, so fetch it here for the sidebar profile card.
    if (!this.account.currentUser()) {
      this.account.refreshUser();
    }

    this.dashboardService.getSummary()
      .pipe(catchError(() => of(null)))
      .subscribe((response) => {
        this.lowStockCount.set(response?.data?.lowStock?.length ?? 0);
      });
  }

  logout(): void {
    this.account.logout().subscribe({
      next: () => {
        this.account.currentUser.set(null);
        this.router.navigate(['/account/login']);
      },
      error: () => {
        this.account.currentUser.set(null);
        this.router.navigate(['/account/login']);
      }
    });
  }

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }

  closeMenu(): void {
    this.menuOpen = false;
  }

  readonly navItems = [
    { link: '/admin', label: 'Dashboard', translationKey: 'ADMIN_DASHBOARD', icon: 'dashboard', exact: true },
    { link: '/admin/products', label: 'Products', translationKey: 'ADMIN_PRODUCTS', icon: 'inventory_2', exact: false },
    { link: '/admin/categories', label: 'Categories', translationKey: 'ADMIN_CATEGORIES', icon: 'category', exact: false },
    { link: '/admin/orders', label: 'Orders', translationKey: 'ADMIN_ORDERS', icon: 'receipt_long', exact: false },
    { link: '/admin/customers', label: 'Customers', translationKey: 'ADMIN_CUSTOMERS', icon: 'group', exact: false },
    { link: '/admin/coupons', label: 'Coupons', translationKey: 'ADMIN_COUPONS', icon: 'percent', exact: false },
    { link: '/admin/locations', label: 'Locations', translationKey: 'ADMIN_LOCATIONS', icon: 'location_on', exact: false },
    { link: '/admin/landing', label: 'Landing Pages', translationKey: 'ADMIN_LANDING', icon: 'web', exact: false },
    { link: '/admin/email-settings', label: 'Email Design', translationKey: 'ADMIN_EMAIL_SETTINGS', icon: 'palette', exact: false },
    { link: '/admin/settings', label: 'Settings', translationKey: 'ADMIN_SETTINGS', icon: 'settings', exact: false },
  ];

  readonly mobileNavItems = [
    { link: '/admin', label: 'Home', translationKey: 'NAV_HOME', icon: 'home', exact: true },
    { link: '/admin/products', label: 'Products', translationKey: 'ADMIN_PRODUCTS', icon: 'inventory_2', exact: false },
    { link: '/admin/orders', label: 'Orders', translationKey: 'ADMIN_ORDERS', icon: 'receipt_long', exact: false },
  ];

  /** Initials shown in the sidebar avatar. */
  get initials(): string {
    const name = this.user()?.displayName;
    if (!name) return 'A';
    return name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0].toUpperCase())
      .join('');
  }
}
