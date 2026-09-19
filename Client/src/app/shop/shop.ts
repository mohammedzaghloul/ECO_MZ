import { Component, Inject, OnDestroy, OnInit, PLATFORM_ID, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ShopService } from './shop.service';
import { BasketService } from '../basket/basket.service';
import { Ipagination } from '../shared/Models/Pagnation';
import { IProduct } from '../shared/Models/product';
import { ICategory } from '../shared/Models/Category/Category.component';
import { debounceTime, distinctUntilChanged, Subject, takeUntil, timeout } from 'rxjs';
import { StoreSettingsService } from '../core/Services/store-settings.service';

@Component({
  selector: 'app-shop',
  standalone: false,
  templateUrl: './shop.html',
  styleUrl: './shop.scss',
})
export class Shop implements OnInit, OnDestroy {
  products = signal<IProduct[]>([]);
  loading = signal<boolean>(true);
  totalCount = signal<number>(0);
  currentPage = signal<number>(1);
  pageSize = signal<number>(12);
  selectedCategory = signal<number | null>(null);
  selectedSort = signal<string>('name');
  sortMenuOpen = signal(false);
  carouselIndex = signal(0);
  private carouselTimer: any;
  categories = signal<ICategory[]>([]);
  categoryMap = signal<Record<number, string>>({});
  private destroy$ = new Subject<void>();
  private activeRequestId = 0;


  constructor(
    public shopService: ShopService,
    public basketService: BasketService,
    public storeSettings: StoreSettingsService,
    private route: ActivatedRoute,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.carouselTimer = setInterval(() => {
        const count = Math.min(this.products().length, 5);
        if (count > 1) this.carouselIndex.update((i) => (i + 1) % count);
      }, 3000);
      this.shopService
        .onSearchInput()
        .pipe(debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$))
        .subscribe(() => this.getAllProduct());

      this.route.queryParamMap
        .pipe(takeUntil(this.destroy$))
        .subscribe((params) => {
          const categoryParam = params.get('categoryId');
          const categoryId = categoryParam ? Number(categoryParam) : null;
          this.selectedCategory.set(Number.isInteger(categoryId) && categoryId > 0 ? categoryId : null);
          this.currentPage.set(1);
          this.getAllProduct();
        });
      this.getCategories();
    }
  }

  ngOnDestroy(): void {
    if (this.carouselTimer) clearInterval(this.carouselTimer);
    this.destroy$.next();
    this.destroy$.complete();
  }

  productImage(product: IProduct): string {
    return this.basketService.imageUrl(product.photos?.[0] ?? '');
  }

  carouselProducts(): IProduct[] {
    return this.products().slice(0, 5);
  }

  getAllProduct() {
    const requestId = ++this.activeRequestId;
    this.loading.set(true);

    const params: any = {};
    const search = this.shopService.searchTerm().trim();
    if (search) params.Search = search;
    if (this.selectedCategory() != null) params.CategoryId = this.selectedCategory();
    if (this.selectedSort()) params.Sort = this.selectedSort();
    params.PageNumber = this.currentPage();
    params.PageSize = this.pageSize();

    this.shopService.getProduct(params).pipe(timeout(10000)).subscribe({
      next: (value: any) => {
        if (requestId !== this.activeRequestId) {
          return;
        }

        const products = value?.products ?? value?.data ?? [];
        this.products.set(products);
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

    // Already a full URL
    if (photo.startsWith('http')) return photo;

    // Normalise backslashes → forward slashes
    const normalised = photo.replace(/\\/g, '/');

    // If the path already contains "Images/" it's a full relative path (uploaded file)
    if (normalised.includes('Images/')) {
      return `${baseUrl}${normalised}?v=2`;
    }

    // Seed data stores just the filename e.g. "product-1.jpg"
    return `${baseUrl}Images/Products/${normalised}?v=2`;
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = this.getPlaceholderSvg();
  }

  private getPlaceholderSvg(): string {
    return 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzAwIiBoZWlnaHQ9IjMwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMzAwIiBoZWlnaHQ9IjMwMCIgZmlsbD0iI2UwZTBlMCIvPjx0ZXh0IHg9IjUwJSIgeT0iNTAlIiBkb21pbmFudC1iYXNlbGluZT0ibWlkZGxlIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBmb250LXNpemU9IjE2IiBmaWxsPSIjOTk5Ij5ObyBJbWFnZTwvdGV4dD48L3N2Zz4=';
  }

  onSearchInput(term: string) {
    this.shopService.emitSearchInput(term);
  }

  onSearch() {
    this.getAllProduct();
  }

  onReset() {
    this.shopService.searchTerm.set('');
    this.selectedCategory.set(null);
    this.selectedSort.set('name');
    this.currentPage.set(1);
    this.getAllProduct();
  }

  onSortChange(value: string) {
    this.selectedSort.set(value);
    this.sortMenuOpen.set(false);
    this.currentPage.set(1);
    this.getAllProduct();
  }

  toggleSortMenu(): void {
    this.sortMenuOpen.update((open) => !open);
  }

  onCategorySelected(categoryId: number | null) {
    this.selectedCategory.set(categoryId);
    this.currentPage.set(1);
    this.getAllProduct();
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
