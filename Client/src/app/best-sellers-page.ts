import { Component, Inject, PLATFORM_ID, OnInit, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ShopService } from './shop/shop.service';
import { IProduct } from './shared/Models/product';
import { ICategory } from './shared/Models/Category/Category.component';
import { Subject, timeout } from 'rxjs';

@Component({
  selector: 'app-best-sellers-page',
  standalone: false,
  templateUrl: './best-sellers-page.html',
  styleUrls: ['./shop/shop.scss', './best-sellers-page.scss'],
})
export class BestSellersPage implements OnInit {
  products = signal<IProduct[]>([]);
  heroBackground = signal<string>('');
  loading = signal<boolean>(true);
  totalCount = signal<number>(0);
  currentPage = signal<number>(1);
  pageSize = signal<number>(12);
  categories = signal<ICategory[]>([]);
  categoryMap = signal<Record<number, string>>({});
  private destroy$ = new Subject<void>();
  private activeRequestId = 0;

  constructor(
    public shopService: ShopService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.getAllProduct();
      this.getCategories();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getAllProduct() {
    const requestId = ++this.activeRequestId;
    this.loading.set(true);

    const params: any = {};
    params.PageNumber = this.currentPage();
    params.PageSize = this.pageSize();
    params.Sort = 'bestSelling';

    this.shopService.getProduct(params).pipe(timeout(10000)).subscribe({
      next: (value: any) => {
        if (requestId !== this.activeRequestId) {
          return;
        }

        const products = value?.products ?? value?.data ?? [];
        this.products.set(products);
        const heroImage = products.length > 0
          ? this.getImageUrl(products[0].photos)
          : '';
        this.heroBackground.set(heroImage
          ? `linear-gradient(90deg, rgba(15, 23, 42, .9) 0%, rgba(15, 23, 42, .68) 48%, rgba(30, 64, 175, .28) 100%), url("${heroImage}")`
          : '');
        this.totalCount.set(value?.totalCount ?? 0);
        this.loading.set(false);
      },
      error: (err) => {
        if (requestId !== this.activeRequestId) {
          return;
        }

        console.error('Error fetching Product:', err);
        this.products.set([]);
        this.loading.set(false);
      }
    });
  }

  getCategories() {
    this.shopService.getCategories().subscribe({
      next: (value: ICategory[]) => {
        this.categories.set(value);
        const map: Record<number, string> = {};
        value.forEach((category) => {
          map[category.id] = category.name;
        });
        this.categoryMap.set(map);
      },
      error: (err) => {
        console.error('Error fetching categories:', err);
      }
    });
  }

  getCategoryName(categoryId: number): string {
    return this.categoryMap()[categoryId] ?? `Category #${categoryId}`;
  }

  calculateDiscount(oldPrice: number, newPrice: number): number {
    if (!oldPrice || !newPrice || oldPrice <= newPrice) {
      return 0;
    }
    const discount = ((oldPrice - newPrice) / oldPrice) * 100;
    return Math.round(discount);
  }

  getImageUrl(photos: string | string[] | undefined): string {
    const baseUrl = this.shopService.BaseUrl().replace('api/', '');

    if (!photos) return this.getPlaceholderSvg();

    const photo = Array.isArray(photos) ? photos[0] : photos;

    if (!photo) return this.getPlaceholderSvg();

    if (photo.startsWith('http')) return photo;

    const normalised = photo.replace(/\\/g, '/');

    if (normalised.includes('Images/')) {
      return `${baseUrl}${normalised}?v=2`;
    }

    return `${baseUrl}Images/Products/${normalised}?v=2`;
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = this.getPlaceholderSvg();
  }

  private getPlaceholderSvg(): string {
    return 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzAwIiBoZWlnaHQ9IjMwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMzAwIiBoZWlnaHQ9IjMwMCIgZmlsbD0iI2UwZTBlMCIvPjx0ZXh0IHg9IjUwJSIgeT0iNTAlIiBkb21pbmFudC1iYXNlbGluZT0ibWlkZGxlIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBmb250LXNpemU9IjE2IiBmaWxsPSIjOTk5Ij5ObyBJbWFnZTwvdGV4dD48L3N2Zz4=';
  }

  totalPages(): number {
    const total = this.totalCount();
    const size = this.pageSize();
    return Math.max(1, Math.ceil(total / size));
  }

  pageNumbers(): number[] {
    const totalPages = this.totalPages();
    const currentPage = this.currentPage();
    const pages: number[] = [];
    const start = Math.max(1, currentPage - 2);
    const end = Math.min(totalPages, start + 4);

    for (let page = start; page <= end; page++) {
      pages.push(page);
    }

    return pages;
  }

  goToPage(page: number): void {
    const totalPages = this.totalPages();
    if (page < 1 || page > totalPages) {
      return;
    }
    this.currentPage.set(page);
    this.getAllProduct();
  }

  getCategoryIcon(name: string): string {
    const icons: Record<string, string> = {
      Electronics: 'fa-laptop',
      Clothing: 'fa-shopping-bag',
      Books: 'fa-book',
    };
    return icons[name] ?? 'fa-tag';
  }
}
