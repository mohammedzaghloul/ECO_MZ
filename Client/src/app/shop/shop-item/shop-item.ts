import { Component, Input } from '@angular/core';
import { IProduct } from '../../shared/Models/product';

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

  calculateDiscount(): number {
    if (!this.product.oldPrice || this.product.oldPrice <= this.product.newPrice) {
      return 0;
    }
    return Math.round(((this.product.oldPrice - this.product.newPrice) / this.product.oldPrice) * 100);
  }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src =
      'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzAwIiBoZWlnaHQ9IjMwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWRkdGg9IjMwMCIgaGVpZ2h0PSIzMDAiIGZpbGw9IiNlMGUwZTAiLz48dGV4dCB4PSI1MCUiIHk9IjUwJSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZmlsbD0iIzk5OSI+Tm8gSW1hZ2U8L3RleHQ+PC9zdmc+';
  }
}
