import { Component, Inject, PLATFORM_ID, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { AccountService } from './core/Services/account.service';
import { BasketService } from './basket/basket.service';
import { OrderService } from './core/Services/order.service';
import { UserDto, AddressDto } from './shared/Models/api/account.models';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CheckoutLocation, LocationService } from './core/Services/location.service';

@Component({
  selector: 'app-account-page',
  standalone: false,
  templateUrl: './account-page.html',
  styleUrl: './account-page.scss',
})
export class AccountPage implements OnInit {
  activeTab = signal<'profile' | 'orders'>('profile');
  currentUser = signal<UserDto | null>(null);
  orders = signal<any[]>([]);
  loadingUser = signal<boolean>(true);
  loadingOrders = signal<boolean>(false);
  isLoggedIn = signal<boolean>(false);
  address = signal<AddressDto | null>(null);
  addressForm!: FormGroup;
  savingAddress = signal(false);
  addressSaved = signal(false);
  editingAddress = signal(false);
  governorates = signal<CheckoutLocation[]>([]);

  /** DisplayName is NULL for users registered before the backend saves a
      name, so fall back to the email prefix instead of "not specified". */
  readonly displayLabel = computed(() => {
    const user = this.currentUser();
    if (!user) return null;
    const name = user.displayName?.trim();
    if (name) return name;
    return user.email ? user.email.split('@')[0] : null;
  });

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private accountService: AccountService,
    private orderService: OrderService,
    public basketService: BasketService,
    private fb: FormBuilder,
    private locationService: LocationService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  ngOnInit(): void {
    const page = this.route.snapshot.data['page'] as string | undefined;
    if (page === 'orders') {
      this.activeTab.set('orders');
    }

    if (isPlatformBrowser(this.platformId)) {
      this.loadCurrentUser();
      this.initAddressForm();
      this.loadAddress();
      this.loadGovernorates();
    }
  }

  initAddressForm(): void {
    this.addressForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      street: ['', Validators.required],
      city: ['', Validators.required],
      state: ['', Validators.required],
      zipCode: ['', [Validators.required, Validators.pattern(/^\d{4,10}$/)]],
      country: ['', Validators.required]
    });

    this.addressForm.get('state')?.valueChanges.subscribe(state => {
      const city = this.addressForm.get('city');
      city?.reset('');
    });
  }

  loadAddress(): void {
    this.accountService.getAddress().subscribe({
      next: (res: any) => {
        const dto = res?.data ?? null;
        this.address.set(dto);
        if (dto) this.addressForm.patchValue(dto);
      },
      error: () => this.address.set(null)
    });
  }

  loadGovernorates(): void {
    this.locationService.getEgypt().subscribe({
      next: locations => {
        this.governorates.set(locations);
      },
      error: () => this.governorates.set([])
    });
  }

  editAddress(): void {
    if (this.address()) this.addressForm.patchValue(this.address()!);
    this.addressSaved.set(false);
    this.editingAddress.set(true);
  }

  saveAddress(): void {
    if (this.addressForm.invalid) { this.addressForm.markAllAsTouched(); return; }
    this.savingAddress.set(true);
    this.accountService.updateAddress(this.addressForm.value).subscribe({
      next: (res: any) => {
        this.address.set(res?.data ?? this.addressForm.value);
        this.savingAddress.set(false);
        this.editingAddress.set(false);
        this.addressSaved.set(true);
        setTimeout(() => this.addressSaved.set(false), 3000);
      },
      error: () => this.savingAddress.set(false)
    });
  }

  isAddrInvalid(field: string): boolean {
    const c = this.addressForm.get(field);
    return !!(c && c.invalid && c.touched);
  }

  loadCurrentUser(): void {
    this.loadingUser.set(true);
    this.accountService.getCurrentUser().subscribe({
      next: (res: any) => {
        const user = res?.data ?? res;
        if (user && (user.email || user.displayName)) {
          this.currentUser.set(user);
          this.isLoggedIn.set(true);
          this.loadOrders();
        } else {
          this.isLoggedIn.set(false);
        }
        this.loadingUser.set(false);
      },
      error: () => {
        this.currentUser.set(null);
        this.isLoggedIn.set(false);
        this.loadingUser.set(false);
      },
    });
  }

  loadOrders(): void {
    this.loadingOrders.set(true);
    this.orderService.getAllOrdersForUser().subscribe({
      next: (res: any) => {
        this.orders.set(res?.data ?? res ?? []);
        this.loadingOrders.set(false);
      },
      error: (err) => {
        console.error('Error fetching orders:', err);
        this.orders.set([]);
        this.loadingOrders.set(false);
      },
    });
  }

  setTab(tab: 'profile' | 'orders'): void {
    this.activeTab.set(tab);
  }

  getOrderStatus(status: string | number): string {
    const statusMap: Record<string, string> = {
      '0': 'Pending',
      '1': 'Payment Received',
      '2': 'Payment Failed',
      'PaymentRecevied': 'Payment Received',
      'PaymentFaild': 'Payment Failed',
      'Pending': 'Pending',
      'Processing': 'Processing',
      'Shipped': 'Shipped',
      'Delivered': 'Delivered',
      'Cancelled': 'Cancelled',
    };
    return statusMap[String(status)] ?? String(status);
  }

  getOrderStatusClass(status: string | number): string {
    const classMap: Record<string, string> = {
      '0': 'badge-pending',
      '1': 'badge-success',
      '2': 'badge-failed',
      'PaymentRecevied': 'badge-success',
      'PaymentFaild': 'badge-failed',
      'Pending': 'badge-pending',
      'Processing': 'badge-info',
      'Shipped': 'badge-primary',
      'Delivered': 'badge-success',
      'Cancelled': 'badge-failed',
    };
    return classMap[String(status)] ?? 'badge-secondary';
  }

  formatDate(dateString: string): string {
    if (!dateString) return 'Recent';
    const date = new Date(dateString);
    return isNaN(date.getTime()) ? dateString : date.toLocaleDateString();
  }

  citiesForGovernorate(): { id: number; value: string; en: string; ar: string; shippingPrice: number; deliveryDays: number; shippingAvailable: boolean }[] {
    return this.governorates().find(location => location.value === this.addressForm?.get('state')?.value)?.cities ?? [];
  }

  localizedOption(option: { en: string; ar: string }): string {
    // Default to English for now - could add LanguageService later
    return option.en;
  }
}
