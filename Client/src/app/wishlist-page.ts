import { Component, OnInit, computed, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { WishlistService } from './shared/Services/wishlist.service';
import { AccountService } from './core/Services/account.service';
import { BasketService } from './basket/basket.service';
import { ShopService } from './shop/shop.service';
import { ToastrService } from 'ngx-toastr';
import { IProduct } from './shared/Models/product';
import { LanguageService } from './core/Services/language.service';

@Component({
  selector: 'app-wishlist-page',
  standalone: false,
  templateUrl: './wishlist-page.html',
  styleUrl: './wishlist-page.scss',
})
export class WishlistPage implements OnInit {
  items = computed(() => this.wishlistService.items());
  totalCount = computed(() => this.items().length);
  loading = computed(() => !this.wishlistService.loaded());
  isLoggedIn = computed(() => !!this.accountService.currentUser());

  constructor(
    public wishlistService: WishlistService,
    private accountService: AccountService,
    public basketService: BasketService,
    private shopService: ShopService,
    private toastr: ToastrService,
    private langService: LanguageService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.accountService.refreshUser();
      this.wishlistService.loadFromServer();
    }
  }

  isWishlisted(productId: number): boolean {
    return this.wishlistService.isInWishlist(productId);
  }

  remove(item: IProduct): void {
    this.wishlistService.toggle(item);
  }

  addToBasket(item: IProduct): void {
    this.basketService.add(item, 1);
    if (isPlatformBrowser(this.platformId)) {
      this.toastr.success(
        this.langService.t('CART_ADDED_MESSAGE', { qty: 1, name: item.name }),
        this.langService.t('CART_UPDATED'),
        {
          timeOut: 1000,
          closeButton: false,
          progressBar: false,
        }
      );
    }
  }

  discountPercent(item: IProduct): number {
    if (!item.oldPrice || item.oldPrice <= item.newPrice) return 0;
    return Math.round(((item.oldPrice - item.newPrice) / item.oldPrice) * 100);
  }

  getImageUrl(photos: string[]): string {
    if (!photos || photos.length === 0) return 'assets/images/product-placeholder.svg';

    const photo = photos[0];
    if (!photo) return 'assets/images/product-placeholder.svg';
    if (photo.startsWith('http')) return photo;

    const baseUrl = this.shopService.BaseUrl().replace('api/', '');
    const normalised = photo.replace(/\\/g, '/');

    if (normalised.includes('Images/')) {
      return `${baseUrl}${normalised}`;
    }
    return `${baseUrl}Images/Products/${normalised}`;
  }

  money(value: number): string {
    return `$${(value ?? 0).toFixed(2)}`;
  }
}
