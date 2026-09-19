import { Component, OnDestroy, OnInit, signal, computed, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ShopService } from './shop/shop.service';
import { IProduct } from './shared/Models/product';

@Component({
  selector: 'app-home-page',
  standalone: false,
  templateUrl: './home-page.html',
  styleUrl: './home-page.scss',
})
export class HomePage implements OnInit, OnDestroy {
  carouselIndex = signal(0);
  private carouselTimer: any;

  showcaseList = computed(() => this.products().slice(0, 5));
  products = signal<IProduct[]>([]);
  categories = signal<any[]>([]);
  heroProduct = computed(() => this.products()[0] ?? null);
  heroThumbs = computed(() => this.products().slice(1, 3));

  constructor(
    private shopService: ShopService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  categoryIcon(category: any): string {
    const name = String(category?.name ?? '').toLowerCase();
    if (name.includes('electr') || name.includes('إلكترون')) return 'devices';
    if (name.includes('cloth') || name.includes('ملابس')) return 'checkroom';
    if (name.includes('book') || name.includes('كتب')) return 'menu_book';
    if (name.includes('home') || name.includes('منزل')) return 'chair';
    if (name.includes('beaut') || name.includes('جمال') || name.includes('جمال')) return 'spa';
    return 'category';
  }

  ngOnDestroy(): void {
    if (this.carouselTimer) clearInterval(this.carouselTimer);
  }

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.carouselTimer = setInterval(() => {
        const count = Math.min(this.products().length, 5);
        if (count > 1) this.carouselIndex.update((i) => (i + 1) % count);
      }, 3500);
    }
    this.shopService.getProduct({ PageNumber: 1, PageSize: 12 }).subscribe({
      next: (response: any) => this.products.set(response?.data ?? response?.products ?? []),
    });
    this.shopService.getCategories().subscribe({
      next: (categories) => this.categories.set(categories.slice(0, 5)),
    });
  }

  imageUrl(product: IProduct): string {
    const photo = Array.isArray(product.photos) ? product.photos[0] : product.photos;
    if (!photo) return 'assets/images/product-placeholder.svg';
    if (photo.startsWith('http')) return photo;

    const baseUrl = this.shopService.BaseUrl().replace('api/', '');
    const normalised = photo.replace(/\\/g, '/');
    if (normalised.includes('Images/')) {
      return `${baseUrl}${normalised}?v=2`;
    }
    return `${baseUrl}Images/Products/${normalised}?v=2`;
  }
}
