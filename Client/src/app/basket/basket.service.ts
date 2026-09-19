import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, PLATFORM_ID, computed, signal } from '@angular/core';
import { tap } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';
import { environment } from '../../environments/environment.development';
import { CustomerBasketDto as IBasket, BasketItemDto as IBasketItem } from '../shared/Models/api/basket.models';
import { CouponResult } from '../core/Services/admin.service';
import { IProduct } from '../shared/Models/product';
import { OfflineSupportService } from '../core/Services/offline-support.service';

@Injectable({ providedIn: 'root' })
export class BasketService {
  private readonly storageKey = 'eco-basket-id';
  private readonly couponKey = 'eco-coupon';
  readonly basket = signal<IBasket | null>(null);
  /** Number of distinct products in the basket (not total quantity). */
  readonly itemCount = computed(() => this.basket()?.basketItems.length ?? 0);
  readonly total = computed(() =>
    this.basket()?.basketItems.reduce((amount, item) => amount + item.price * item.quantity, 0) ?? 0
  );
  /** Applied coupon, kept in sync with the server-side basket. */
  readonly coupon = signal<{ code: string; isPercentage: boolean; value: number; discountAmount: number } | null>(null);
  readonly couponError = signal<string | null>(null);
  readonly couponApplying = signal(false);
  readonly offlineMessage = signal('');
  readonly discountAmount = computed(() => Math.min(this.coupon()?.discountAmount ?? 0, this.total()));

  constructor(
    private http: HttpClient,
    private offlineSupport: OfflineSupportService,
    @Inject(PLATFORM_ID) private platformId: object,
  ) {
    if (isPlatformBrowser(this.platformId)) {
      try {
        const stored = localStorage.getItem(this.couponKey);
        if (stored) this.coupon.set(JSON.parse(stored));
      } catch {
        localStorage.removeItem(this.couponKey);
      }
      const basketId = localStorage.getItem(this.storageKey);
      if (basketId) {
        void this.offlineSupport.getCachedBasket<IBasket>(basketId).then((basket) => {
          if (basket?.basketItems?.length && !this.basket()) this.basket.set(basket);
        });
      }
    }
  }

  private setCoupon(coupon: { code: string; isPercentage: boolean; value: number; discountAmount: number } | null): void {
    this.coupon.set(coupon);
    if (isPlatformBrowser(this.platformId)) {
      if (coupon) localStorage.setItem(this.couponKey, JSON.stringify(coupon));
      else localStorage.removeItem(this.couponKey);
    }
  }

  applyCoupon(code: string): void {
    const normalizedCode = code.trim();
    this.couponError.set(null);
    if (!normalizedCode) {
      this.couponError.set('Please enter a coupon code.');
      return;
    }

    const basketId = this.basket()?.id ?? this.getOrCreateId();
    this.couponApplying.set(true);
    this.http
      .post<CouponResult>(`${environment.basurl}Basket/coupon`, { basketId, code: normalizedCode }, { withCredentials: true })
      .subscribe({
        next: (result) => {
          this.couponApplying.set(false);
          if (result?.success && result.code) {
            this.setCoupon({
              code: result.code,
              isPercentage: !!result.isPercentage,
              value: result.value ?? 0,
              discountAmount: result.discountAmount ?? 0,
            });
            return;
          }
          this.couponError.set(result?.message || 'This coupon could not be applied.');
        },
        error: (error) => {
          this.couponApplying.set(false);
          this.couponError.set(error?.error?.message || 'This coupon could not be applied.');
        },
      });
  }

  removeCoupon(): void {
    this.couponError.set(null);
    const basketId = this.basket()?.id;
    this.setCoupon(null);
    if (basketId) {
      this.http
        .delete<CouponResult>(`${environment.basurl}Basket/coupon`, { params: { basketId }, withCredentials: true })
        .subscribe({ error: () => undefined });
    }
  }

  load(): void {
    if (!isPlatformBrowser(this.platformId) || this.basket()) return;
    const id = localStorage.getItem(this.storageKey);
    if (!id) return;
    this.http.get<IBasket | null>(`${environment.basurl}Basket`, { params: { id } }).subscribe({
      next: (basket) => this.basket.set(basket?.basketItems ? basket : null),
      error: () => localStorage.removeItem(this.storageKey),
    });
  }

  add(product: IProduct, quantity: number = 1): void {
    const qty = Math.max(1, quantity);
    const current = this.basket() ?? { id: this.getOrCreateId(), basketItems: [] };
    const existing = current.basketItems.find((item) => item.id === product.id);
    const basketItems: IBasketItem[] = existing
      ? current.basketItems.map((item) => item.id === product.id ? { ...item, quantity: item.quantity + qty } : item)
      : [...current.basketItems, {
          id: product.id, name: product.name, quantity: qty, image: product.photos?.[0] ?? '',
          price: product.newPrice, category: String(product.categoryId),
        }];
    this.save({ ...current, basketItems });
  }

  setQuantity(productId: number, quantity: number): void {
    const current = this.basket();
    if (!current) return;
    const basketItems = quantity <= 0
      ? current.basketItems.filter((item) => item.id !== productId)
      : current.basketItems.map((item) => item.id === productId ? { ...item, quantity } : item);
    this.save({ ...current, basketItems });
  }

  clear(): void {
    const current = this.basket();
    if (!current) return;
    this.http.delete(`${environment.basurl}Basket`, { params: { id: current.id } }).subscribe({
      next: () => this.reset(),
      error: () => this.reset(),
    });
  }

  /** Recomputes the stored coupon preview against the current basket total. */
  private refreshCouponPreview(): void {
    const coupon = this.coupon();
    if (!coupon) return;
    const discountAmount = coupon.isPercentage
      ? Math.round(this.total() * (coupon.value / 100) * 100) / 100
      : Math.min(coupon.value, this.total());
    this.setCoupon({ ...coupon, discountAmount });
  }

  imageUrl(image: string): string {
    if (!image) return 'assets/images/product-placeholder.svg';
    if (image.startsWith('http')) return image;
    const baseUrl = environment.basurl.replace('api/', '');
    const normalised = image.replace(/\\/g, '/');
    if (normalised.includes('Images/')) {
      return `${baseUrl}${normalised}?v=2`;
    }
    return `${baseUrl}Images/Products/${normalised}?v=2`;
  }

  private save(basket: IBasket): void {
    this.basket.set(basket);
    this.offlineMessage.set('');
    void this.offlineSupport.cacheBasket(basket, basket.id);
    const request = {
      id: basket.id,
      couponCode: this.coupon()?.code ?? null,
      items: basket.basketItems.map((item) => ({
        productId: item.id,
        quantity: item.quantity,
      })),
    };
    this.http.post<IBasket>(`${environment.basurl}Basket`, request, { withCredentials: true }).pipe(tap((saved) => {
      if (saved?.basketItems) this.basket.set(saved);
      this.refreshCouponPreview();
    })).subscribe({
      error: async (error) => {
        await this.offlineSupport.refreshStatusAndFlush();
        await this.offlineSupport.queueBasket(request, basket.id);
        this.offlineMessage.set(this.offlineSupport.describeFailure(error));
      },
    });
  }

  private getOrCreateId(): string {
    if (!isPlatformBrowser(this.platformId)) return 'guest';
    const saved = localStorage.getItem(this.storageKey);
    if (saved) return saved;
    const id = uuidv4();
    localStorage.setItem(this.storageKey, id);
    return id;
  }

  private reset(): void {
    this.basket.set(null);
    this.setCoupon(null);
    this.couponError.set(null);
    this.couponApplying.set(false);
    if (isPlatformBrowser(this.platformId)) localStorage.removeItem(this.storageKey);
  }
}
