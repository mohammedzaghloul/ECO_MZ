import { Component, effect, HostListener, Inject, OnDestroy, OnInit, PLATFORM_ID, signal } from '@angular/core';
import { Router } from '@angular/router';
import { BasketService } from '../../../basket/basket.service';
import { WishlistService } from '../../Services/wishlist.service';
import { LanguageService } from '../../../core/Services/language.service';
import { AccountService } from '../../../core/Services/account.service';
import { ShopService } from '../../../shop/shop.service';
import { IProduct } from '../../Models/product';
import { isPlatformBrowser } from '@angular/common';
import { NotificationItem, NotificationService } from '../../../core/Services/notification.service';

interface NavLink {
  key: string;
  path: string;
  fragment?: string;
  exact?: boolean;
  icon: string;
}

@Component({
  selector: 'app-header',
  standalone: false,
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnDestroy, OnInit {
  isMobileMenuOpen = false;
  isScrolled = false;
  isUserMenuOpen = false;
  newArrivals = signal<IProduct[]>([]);
  notifications = signal<NotificationItem[]>([]);
  isNotificationsOpen = false;
  private notificationRefreshTimer?: ReturnType<typeof setInterval>;

  navLinks: NavLink[] = [
    { key: 'NAV_HOME', path: '/', exact: true, icon: 'home' },
    { key: 'NAV_SHOP', path: '/shop', exact: true, icon: 'storefront' },
    { key: 'NAV_BEST_SELLERS', path: '/best-sellers', exact: true, icon: 'star' },
    { key: 'NAV_ABOUT', path: '/about', exact: true, icon: 'info' },
    { key: 'NAV_CONTACT', path: '/about', fragment: 'contact', exact: false, icon: 'mail' }
  ];

  constructor(
    private router: Router,
    public basketService: BasketService,
    public wishlistService: WishlistService,
    public langService: LanguageService,
    public accountService: AccountService,
    private shopService: ShopService,
    public notificationService: NotificationService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {
    this.accountService.refreshUser();
    effect(() => {
      if (this.accountService.currentUser()) {
        this.notificationService.loadUnreadCount();
        if (isPlatformBrowser(this.platformId) && !this.notificationRefreshTimer) {
          this.notificationRefreshTimer = setInterval(
            () => this.notificationService.loadUnreadCount(),
            15000
          );
        }
      } else if (this.notificationRefreshTimer) {
        clearInterval(this.notificationRefreshTimer);
        this.notificationRefreshTimer = undefined;
        this.notificationService.unreadCount.set(0);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.notificationRefreshTimer) {
      clearInterval(this.notificationRefreshTimer);
    }
  }

  toggleNotifications(event: Event): void {
    event.stopPropagation();
    this.isNotificationsOpen = !this.isNotificationsOpen;
    if (this.isNotificationsOpen) {
      this.notificationService.getAll().subscribe({
        next: response => {
          const items = response?.data ?? [];
          this.notifications.set(items);
          this.notificationService.updateCountFromItems(items);
        }
      });
    }
  }

  markNotificationAsRead(notification: NotificationItem): void {
    const openRelatedOrder = () => {
      this.isNotificationsOpen = false;
      if (notification.type.toLowerCase() === 'order' && notification.relatedEntityId) {
        this.router.navigate(['/my-order', notification.relatedEntityId]);
      }

    };

    if (notification.isRead) {
      openRelatedOrder();
      return;
    }

    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        this.notifications.update(items =>
          items.map(item => item.id === notification.id ? { ...item, isRead: true } : item)
        );
        this.notificationService.updateCountFromItems(this.notifications());
        openRelatedOrder();
      }
    });
  }

  clearNotifications(): void {
    if (!this.notifications().length) {
      return;
    }

    this.notificationService.clearAll().subscribe({
      next: () => {
        this.notifications.set([]);
        this.notificationService.unreadCount.set(0);
      }
    });
  }

  notificationTitle(notification: NotificationItem): string {
    const title = notification.title.trim().toLowerCase();
    if (notification.type.toLowerCase() === 'order' && title.includes('created')) {
      return this.langService.t('NOTIFICATION_ORDER_CREATED_TITLE');
    }
    if (notification.type.toLowerCase() === 'order' && title.includes('status')) {
      return this.langService.t('NOTIFICATION_ORDER_STATUS_TITLE');
    }
    return notification.title;
  }

  notificationMessage(notification: NotificationItem): string {
    const title = notification.title.trim().toLowerCase();
    const orderId = notification.message.match(/#(\d+)/)?.[1];
    if (notification.type.toLowerCase() === 'order' && title.includes('created') && orderId) {
      return this.langService.t('NOTIFICATION_ORDER_CREATED_MESSAGE', { id: orderId });
    }
    if (notification.type.toLowerCase() === 'order' && title.includes('status') && orderId) {
      const status = notification.message.toLowerCase();
      const statusKey = status.includes('shipped')
        ? 'NOTIFICATION_STATUS_SHIPPED' : status.includes('delivered')
          ? 'NOTIFICATION_STATUS_DELIVERED' : status.includes('payment received')
            ? 'NOTIFICATION_STATUS_PAYMENT_RECEIVED' : status.includes('payment failed')
              ? 'NOTIFICATION_STATUS_PAYMENT_FAILED' : 'NOTIFICATION_STATUS_PENDING';
      return this.langService.t('NOTIFICATION_ORDER_STATUS_MESSAGE', {
        id: orderId,
        status: this.langService.t(statusKey)
      });
    }
    return notification.message;
  }

  notificationDate(notification: NotificationItem): string {
    return new Intl.DateTimeFormat(
      this.langService.currentLang() === 'ar' ? 'ar-EG' : 'en-US',
      { dateStyle: 'medium', timeStyle: 'short' }
    ).format(new Date(notification.createdAt));
  }

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.shopService.getProduct({ PageNumber: 1, PageSize: 3 }).subscribe({
      next: (res: any) => this.newArrivals.set(res?.data ?? res?.products ?? []),
      error: () => this.newArrivals.set([])
    });
  }

  productImage(product: IProduct): string {
    return this.basketService.imageUrl(product.photos?.[0] ?? '');
  }

  toggleLanguage(): void {
    this.langService.toggleLanguage();
  }

  isLinkActive(link: NavLink): boolean {
    const currentUrl = this.router.url;
    const hasFragment = currentUrl.includes('#');

    if (link.fragment) {
      return currentUrl.endsWith('#' + link.fragment) || currentUrl.includes('#' + link.fragment);
    }

    if (link.path === '/shop' && hasFragment) {
      return false;
    }

    if (link.path === '/about' && hasFragment) {
      return false;
    }

    if (link.path === '/') {
      return currentUrl === '/' || currentUrl === '';
    }

    return currentUrl.split('?')[0].split('#')[0] === link.path;
  }

  @HostListener('window:scroll')
  onWindowScroll(): void {
    this.isScrolled = window.scrollY > 8;
  }

  @HostListener('window:focus')
  refreshNotificationsOnFocus(): void {
    if (this.accountService.currentUser()) {
      this.notificationService.loadUnreadCount();
    }
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }

  toggleUserMenu(event: Event): void {
    event.stopPropagation();
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  @HostListener('document:click')
  closeUserMenu(): void {
    this.isUserMenuOpen = false;
    this.isNotificationsOpen = false;
  }

  logout(): void {
    this.isUserMenuOpen = false;
    this.accountService.logout().subscribe({
      next: () => {
        this.accountService.currentUser.set(null);
        this.router.navigate(['/']);
      },
      error: () => {
        this.accountService.currentUser.set(null);
        this.router.navigate(['/']);
      }
    });
  }

  isAdmin(): boolean {
    const user = this.accountService.currentUser();
    return user?.roles?.includes('Admin') ?? false;
  }
}