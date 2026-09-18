import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { IProduct } from '../../shared/Models/product';
import { ShopService } from '../shop.service';
import { finalize, timeout } from 'rxjs';

@Component({
  selector: 'app-product-details',
  standalone: false,
  templateUrl: './product-details.html',
  styleUrl: './product-details.scss',
})
export class ProductDetails implements OnInit {
  product = signal<IProduct | undefined>(undefined);
  loading = signal(true);
  errorMessage = signal('');

  constructor(
    private route: ActivatedRoute,
    private shopService: ShopService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(id) || id < 1) {
      this.loading.set(false);
      return;
    }

    this.shopService
      .getProductById(id)
      .pipe(
        timeout(10000),
        finalize(() => {
          this.loading.set(false);
        })
      )
      .subscribe({
        next: (response) => {
          this.product.set((response as any)?.data ?? response);
        },
        error: (error) => {
          console.error('Error fetching product details:', error);
          this.errorMessage.set('Unable to load this product. Please try again.');
        },
      });
  }

  getImageUrl(): string {
    const photo = this.product()?.photos?.[0];
    if (!photo) return '';
    return photo.startsWith('http')
      ? photo
      : `${this.shopService.BaseUrl().replace('api/', '')}Images/Products/${photo}`;
  }

  calculateDiscount(): number {
    const p = this.product();
    if (!p || !p.oldPrice || p.oldPrice <= p.newPrice) return 0;
    return Math.round(((p.oldPrice - p.newPrice) / p.oldPrice) * 100);
  }
}
