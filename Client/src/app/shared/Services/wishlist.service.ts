import { Injectable, signal, computed, effect, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { IProduct } from '../Models/product';
import { AccountService } from '../../core/Services/account.service';
import { environment } from '../../../environments/environment.development';
import { ApiEnvelope } from '../Models/api/common.models';
import { ToastrService } from 'ngx-toastr';

/** Matches ECO.BLL.DTO.WishlistDtos.WishlistItemDto. */
export interface WishlistItemDto {
  productId: number;
  name: string;
  newPrice: number;
  oldPrice: number;
  photo?: string | null;
  addedAt: string;
}

@Injectable({ providedIn: 'root' })
export class WishlistService {
  private readonly baseUrl = `${environment.basurl}Wishlist`;
  readonly items = signal<IProduct[]>([]);
  readonly count = computed(() => this.items().length);
  /** Becomes true after the first wishlist fetch resolves (success or failure). */
  readonly loaded = signal(false);

  constructor(
    private http: HttpClient,
    private accountService: AccountService,
    private toastr: ToastrService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: object
  ) {
    // Keep the wishlist in sync with the auth state: load on login, clear on logout.
    effect(() => {
      const user = this.accountService.currentUser();
      if (user) {
        this.loadFromServer();
      } else {
        this.items.set([]);
        this.loaded.set(false);
      }
    });
  }

  loadFromServer(): void {
    this.http
      .get<ApiEnvelope<WishlistItemDto[]>>(this.baseUrl, { withCredentials: true })
      .subscribe({
        next: (response) => {
          this.items.set((response?.data ?? []).map((dto) => this.toProduct(dto)));
          this.loaded.set(true);
        },
        error: () => {
          this.items.set([]);
          this.loaded.set(true);
        },
      });
  }

  isInWishlist(productId: number): boolean {
    return this.items().some((p) => p.id === productId);
  }

  /** Adds/removes the product optimistically; returns true when it was added. */
  toggle(product: IProduct): boolean {
    if (!isPlatformBrowser(this.platformId)) return false;

    const user = this.accountService.currentUser();
    if (!user) {
      this.toastr.info('Sign in to save items to your wishlist.', 'Sign In Required');
      this.router.navigate(['/account/login']);
      return false;
    }

    const exists = this.isInWishlist(product.id);

    if (exists) {
      this.items.update((list) => list.filter((p) => p.id !== product.id));
    } else {
      this.items.update((list) => [...list, product]);
    }

    const request$ = exists
      ? this.http.delete<ApiEnvelope<never>>(`${this.baseUrl}/${product.id}`, { withCredentials: true })
      : this.http.post<ApiEnvelope<never>>(`${this.baseUrl}/${product.id}`, {}, { withCredentials: true });

    request$.subscribe({
      error: () => {
        // Revert the optimistic change and inform the user.
        if (exists) {
          this.items.update((list) => [...list, product]);
        } else {
          this.items.update((list) => list.filter((p) => p.id !== product.id));
        }
        this.toastr.error('Could not update your wishlist. Please try again.', 'Wishlist');
      },
    });

    return !exists;
  }

  private toProduct(dto: WishlistItemDto): IProduct {
    return {
      id: dto.productId,
      name: dto.name,
      newPrice: dto.newPrice,
      oldPrice: dto.oldPrice,
      categoryId: 0,
      photos: dto.photo ? [dto.photo] : [],
      description: '',
    };
  }
}
