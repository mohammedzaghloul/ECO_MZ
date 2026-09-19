import { isPlatformBrowser } from '@angular/common';
import { Component, Input, PLATFORM_ID, inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { IProduct } from '../../shared/Models/product';
import { BasketService } from '../../basket/basket.service';
import { WishlistService } from '../../shared/Services/wishlist.service';
import { LanguageService } from '../../core/Services/language.service';
import { StoreSettingsService } from '../../core/Services/store-settings.service';

@Component({
  selector: 'app-shop-item',
  standalone: false,
  templateUrl: './shop-item.html',
  styleUrl: './shop-item.scss',
})
export class ShopItem {
  @Input({ required: true }) product!: IProduct;
  @Input() categoryName = '';
  @Input() imageUrl = '';

  private readonly basketService = inject(BasketService);
  public readonly wishlistService = inject(WishlistService);
  private readonly toastr = inject(ToastrService);
  private readonly languageService = inject(LanguageService);
  public readonly storeSettings = inject(StoreSettingsService);
  private readonly platformId = inject(PLATFORM_ID);

  addToBasket(): void {
    this.basketService.add(this.product);
    if (isPlatformBrowser(this.platformId)) {
      this.toastr.success(
        `"${this.product.name}" ${this.languageService.t('PRODUCT_ADDED_TO_BASKET')}`,
        this.languageService.t('SHOP_ADD_TO_CART'),
        { timeOut: 3000, closeButton: true, progressBar: true }
      );
    }
  }

  toggleWishlist(event: Event): void {
    event.stopPropagation();
    this.wishlistService.toggle(this.product);
  }

  isWishlisted(): boolean {
    return this.wishlistService.isInWishlist(this.product.id);
  }

  calculateDiscount(): number {
    if (!this.product.oldPrice || this.product.oldPrice <= this.product.newPrice) {
      return 0;
    }
    return Math.round(((this.product.oldPrice - this.product.newPrice) / this.product.oldPrice) * 100);
  }

  stars(): string {
    const rating = Math.round(Math.min(5, Math.max(0, this.product.averageRating ?? 0)));
    return '★'.repeat(rating) + '☆'.repeat(5 - rating);
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (!img.src.includes('/images/')) {
      const filename = this.imageUrl ? this.imageUrl.split('/').pop()?.split('?')[0] : '';
      if (filename && !filename.startsWith('data:')) {
        img.src = `images/${filename}`;
        return;
      }
    }
    img.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzAwIiBoZWlnaHQ9IjMwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWRkdGg9IjMwMCIgaGVpZ2h0PSIzMDAiIGZpbGw9IiNlMGUwZTAiLz48dGV4dCB4PSI1MCUiIHk9IjUwJSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZmlsbD0iIzk5OSI+Tm8gSW1hZ2U8L3RleHQ+PC9zdmc+';
  }
}