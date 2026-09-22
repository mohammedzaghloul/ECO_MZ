import { Component, OnInit, signal, computed, Inject, PLATFORM_ID } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { IProduct } from '../../shared/Models/product';
import { ShopService } from '../shop.service';
import { CategoryService } from '../../core/Services/category.service';
import { ProductService } from '../../core/Services/product.service';
import { AccountService } from '../../core/Services/account.service';
import { ReviewDto, ReviewService } from '../../core/Services/review.service';
import { WishlistService } from '../../shared/Services/wishlist.service';
import { BasketService } from '../../basket/basket.service';
import { ToastrService } from 'ngx-toastr';
import { finalize, timeout } from 'rxjs';
import { LanguageService } from '../../core/Services/language.service';

export interface IReview {
  id: number;
  productId: number;
  userName: string;
  rating: number;
  title: string;
  comment: string;
  date: string;
  verified: boolean;
}

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

  selectedPhotoIndex = signal<number>(0);
  quantity = signal<number>(1);
  addedToCart = signal<boolean>(false);
  activeTab = signal<'desc' | 'reviews'>('desc');

  // Review state
  reviews = signal<IReview[]>([]);
  reviewsLoading = signal<boolean>(false);
  submittingReview = signal<boolean>(false);
  newReviewRating = signal<number>(5);
  hoverRating = signal<number>(0);
  newReviewTitle = signal<string>('');
  newReviewComment = signal<string>('');
  showReviewForm = signal<boolean>(false);
  isLoggedIn = computed(() => !!this.accountService.currentUser());
  currentUserName = computed(() => this.accountService.currentUser()?.displayName ?? '');

  // Computed reviews metrics
  reviewsCount = computed(() => this.reviews().length);

  averageRating = computed(() => {
    const list = this.reviews();
    if (list.length === 0) return 0;
    const sum = list.reduce((acc, r) => acc + r.rating, 0);
    return Number((sum / list.length).toFixed(1));
  });

  averageFullStars = computed(() => Math.round(this.averageRating()));

  ratingBreakdown = computed(() => {
    const list = this.reviews();
    const total = list.length;
    const counts = [0, 0, 0, 0, 0]; // 5, 4, 3, 2, 1 stars
    list.forEach((r) => {
      const idx = 5 - Math.min(5, Math.max(1, Math.round(r.rating)));
      counts[idx]++;
    });

    return [5, 4, 3, 2, 1].map((stars, idx) => ({
      stars,
      count: counts[idx],
      percent: total > 0 ? Math.round((counts[idx] / total) * 100) : 0,
    }));
  });

  // Computed list of photo URLs
  photosList = computed<string[]>(() => {
    const p = this.product();
    if (!p) return [];

    let rawList: string[] = [];
    if (Array.isArray(p.photos) && p.photos.length > 0) {
      rawList = p.photos;
    } else if (typeof (p as any).photo === 'string' && (p as any).photo) {
      rawList = [(p as any).photo];
    } else if (typeof p.photos === 'string' && p.photos) {
      rawList = [p.photos as string];
    }

    if (rawList.length === 0) {
      return ['assets/images/product-placeholder.svg'];
    }

    return rawList.map((photo) => this.resolveImageUrl(photo));
  });

  currentMainImage = computed<string>(() => {
    const list = this.photosList();
    const idx = this.selectedPhotoIndex();
    if (list.length === 0) return 'assets/images/product-placeholder.svg';
    return list[idx] || list[0];
  });

  categoryNames = signal<Map<number, string>>(new Map());

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private shopService: ShopService,
    private categoryService: CategoryService,
    private productService: ProductService,
    private reviewService: ReviewService,
    private accountService: AccountService,
    public wishlistService: WishlistService,
    private basketService: BasketService,
    private toastr: ToastrService,
    private langService: LanguageService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  ngOnInit(): void {
    this.loadCategoryNames();
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));
      if (!Number.isInteger(id) || id < 1) {
        this.loading.set(false);
        this.errorMessage.set('Invalid product ID');
        return;
      }

      this.loadProduct(id);
      this.loadReviews(id);
    });
  }

  categoryLabel(): string {
    const p = this.product();
    if (!p) return '';
    const name = this.categoryNames().get(p.categoryId) ??
      ({ 1: 'Electronics', 2: 'Clothing', 3: 'Books' } as Record<number, string>)[p.categoryId] ??
      '';

    const isArabic = isPlatformBrowser(this.platformId)
      ? document.documentElement.dir === 'rtl'
      : false;

    if (isArabic) {
      const localizedNames: Record<string, string> = {
        electronics: 'إلكترونيات',
        clothing: 'ملابس',
        books: 'كتب',
      };
      return localizedNames[name.trim().toLowerCase()] ?? name;
    }

    return name || 'Category';
  }

  private loadCategoryNames(): void {
    if (this.categoryNames().size > 0) return;
    this.categoryService.getAll().subscribe({
      next: (response) => {
        const map = new Map<number, string>();
        for (const c of response?.data ?? []) map.set(c.id, c.name);
        this.categoryNames.set(map);
      },
      error: () => undefined,
    });
  }

  loadProduct(id: number): void {
    this.loading.set(true);
    this.errorMessage.set('');
    this.selectedPhotoIndex.set(0);
    this.quantity.set(1);

    this.productService
      .getById(id)
      .pipe(
        timeout(10000),
        finalize(() => {
          this.loading.set(false);
        })
      )
      .subscribe({
        next: (response: any) => {
          const data = response?.data ?? response;
          this.product.set(data);
        },
        error: (error) => {
          console.error('Error fetching product details:', error);
          this.errorMessage.set('Unable to load this product. Please try again.');
        },
      });
  }

  loadReviews(productId: number): void {
    this.reviewsLoading.set(true);

    this.reviewService.getForProduct(productId).subscribe({
      next: (response) => {
        const data = response?.data ?? [];
        this.reviews.set(data.map((dto) => this.toReviewView(dto)));
        this.reviewsLoading.set(false);
      },
      error: (error) => {
        console.error('Error fetching reviews:', error);
        this.reviews.set([]);
        this.reviewsLoading.set(false);
      },
    });
  }

  private toReviewView(dto: ReviewDto): IReview {
    return {
      id: dto.id,
      productId: dto.productId,
      userName: dto.userName,
      rating: dto.rating,
      title: dto.title ?? 'Verified Customer Review',
      comment: dto.comment,
      date: this.formatReviewDate(dto.createdAt),
      verified: dto.verified,
    };
  }

  private formatReviewDate(isoDate: string): string {
    const date = new Date(isoDate);
    if (Number.isNaN(date.getTime())) return '';

    const minutes = Math.floor((Date.now() - date.getTime()) / 60000);
    if (minutes < 1) return 'Just now';
    if (minutes < 60) return `${minutes} minute${minutes === 1 ? '' : 's'} ago`;

    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours} hour${hours === 1 ? '' : 's'} ago`;

    const days = Math.floor(hours / 24);
    if (days < 7) return `${days} day${days === 1 ? '' : 's'} ago`;

    const weeks = Math.floor(days / 7);
    if (weeks < 5) return `${weeks} week${weeks === 1 ? '' : 's'} ago`;

    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }

  resolveImageUrl(photo: string): string {
    if (!photo) return 'assets/images/product-placeholder.svg';
    if (photo.startsWith('http')) return photo;

    const baseUrl = this.shopService.BaseUrl().replace('api/', '');
    const normalised = photo.replace(/\\/g, '/');

    if (normalised.includes('Images/')) {
      return `${baseUrl}${normalised}?v=2`;
    }
    return `${baseUrl}Images/Products/${normalised}?v=2`;
  }

  selectPhoto(index: number): void {
    if (index >= 0 && index < this.photosList().length) {
      this.selectedPhotoIndex.set(index);
    }
  }

  nextPhoto(): void {
    const total = this.photosList().length;
    if (total <= 1) return;
    this.selectedPhotoIndex.update((curr) => (curr + 1) % total);
  }

  prevPhoto(): void {
    const total = this.photosList().length;
    if (total <= 1) return;
    this.selectedPhotoIndex.update((curr) => (curr - 1 + total) % total);
  }

  increaseQuantity(): void {
    this.quantity.update((q) => Math.min(99, q + 1));
  }

  decreaseQuantity(): void {
    this.quantity.update((q) => Math.max(1, q - 1));
  }

  calculateDiscount(): number {
    const p = this.product();
    if (!p || !p.oldPrice || p.oldPrice <= p.newPrice) return 0;
    return Math.round(((p.oldPrice - p.newPrice) / p.oldPrice) * 100);
  }

  addToBasket(): void {
    const p = this.product();
    if (!p) return;

    const qty = this.quantity();
    this.basketService.add(p, qty);

    this.addedToCart.set(true);
    if (isPlatformBrowser(this.platformId)) {
      this.toastr.success(
        this.langService.t('CART_ADDED_MESSAGE', { qty, name: p.name }),
        this.langService.t('CART_UPDATED'),
        {
          timeOut: 1000,
          closeButton: false,
          progressBar: false,
      });
    }

    setTimeout(() => {
      this.addedToCart.set(false);
    }, 2500);
  }

  isWishlisted(): boolean {
    const p = this.product();
    return !!p && this.wishlistService.isInWishlist(p.id);
  }

  toggleFavorite(): void {
    const p = this.product();
    if (!p) return;

    const added = this.wishlistService.toggle(p);
    if (added && isPlatformBrowser(this.platformId)) {
      this.toastr.success(`"${p.name}" saved to your wishlist.`, 'Wishlist');
    }
  }

  setTab(tab: 'desc' | 'reviews'): void {
    this.activeTab.set(tab);
  }

  openReviewsTab(): void {
    this.activeTab.set('reviews');
    // Smooth scroll to reviews section
    if (isPlatformBrowser(this.platformId)) {
      setTimeout(() => {
        const el = document.getElementById('product-tabs-anchor');
        if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }, 100);
    }
  }

  setRating(rating: number): void {
    this.newReviewRating.set(rating);
  }

  setHoverRating(rating: number): void {
    this.hoverRating.set(rating);
  }

  toggleReviewForm(): void {
    if (!this.showReviewForm() && !this.isLoggedIn()) {
      if (isPlatformBrowser(this.platformId)) {
        this.toastr.info('Please sign in to write a review.', 'Sign In Required');
      }
      this.router.navigate(['/account/login']);
      return;
    }
    this.showReviewForm.update((val) => !val);
  }

  submitReview(): void {
    const user = this.accountService.currentUser();
    if (!user) {
      if (isPlatformBrowser(this.platformId)) {
        this.toastr.warning('Please sign in to write a review.', 'Sign In Required');
      }
      this.router.navigate(['/account/login']);
      return;
    }

    const comment = this.newReviewComment().trim();
    if (!comment) {
      if (isPlatformBrowser(this.platformId)) {
        this.toastr.warning('Please write your review before submitting.', 'Missing Info');
      }
      return;
    }

    const p = this.product();
    if (!p) return;

    this.submittingReview.set(true);
    this.reviewService
      .add({
        productId: p.id,
        rating: this.newReviewRating(),
        title: this.newReviewTitle().trim() || undefined,
        comment,
      })
      .pipe(finalize(() => this.submittingReview.set(false)))
      .subscribe({
        next: (response) => {
          const dto = response?.data;
          if (dto) {
            this.reviews.update((list) => [this.toReviewView(dto), ...list]);
          }

          if (isPlatformBrowser(this.platformId)) {
            this.toastr.success('Thank you! Your review has been published.', 'Review Submitted');
          }

          this.newReviewTitle.set('');
          this.newReviewComment.set('');
          this.newReviewRating.set(5);
          this.showReviewForm.set(false);
        },
        error: (error) => {
          const message =
            error?.error?.message ?? 'Unable to submit your review. Please try again.';
          if (isPlatformBrowser(this.platformId)) {
            this.toastr.error(message, 'Review Failed');
          }
        },
      });
  }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src =
      'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAwIiBoZWlnaHQ9IjQwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWRkdGg9IjQwMCIgaGVpZ2h0PSI0MDAiIGZpbGw9IiNmMWY1ZjkiLz48dGV4dCB4PSI1MCUiIHk9IjUwJSIgZG9taW5hbnQtYmFzZWxpbmU9Im1pZGRsZSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZm9udC1zaXplPSIxNiIgZmlsbD0iIzk0YTNiOSI+Tm8gSW1hZ2U8L3RleHQ+PC9zdmc+';
  }
}
