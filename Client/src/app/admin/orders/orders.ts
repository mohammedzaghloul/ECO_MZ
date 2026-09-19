import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AdminOrder, AdminService } from '../../core/Services/admin.service';
import { LanguageService } from '../../core/Services/language.service';

@Component({
  selector: 'app-admin-orders',
  standalone: false,
  templateUrl: './orders.html',
  styleUrl: './orders.scss',
})
export class AdminOrders implements OnInit, OnDestroy {
  readonly statusOptions = [
    { value: '', labelKey: 'ADMIN_ALL_STATUSES' },
    { value: 'Pending', labelKey: 'ADMIN_PENDING' },
    { value: 'PaymentRecevied', labelKey: 'ADMIN_PAID' },
    { value: 'PaymentFaild', labelKey: 'ADMIN_PAYMENT_FAILED' },
    { value: 'Shipped', labelKey: 'ADMIN_SHIPPED' },
    { value: 'Delivered', labelKey: 'ADMIN_DELIVERED' },
  ];

  orders = signal<AdminOrder[]>([]);
  totalCount = signal(0);
  pageNumber = signal(1);
  pageSize = 10;
  statusFilter = signal('');
  search = signal('');
  fromDate = signal('');
  toDate = signal('');
  loading = signal(true);
  updatingId = signal<number | null>(null);
  selectedOrder = signal<AdminOrder | null>(null);
  detailsLoading = signal(false);
  openStatusId = signal<number | null>(null);
  statusFilterOpen = signal(false);
  detailStatusOpen = signal(false);
  private refreshTimer: ReturnType<typeof setInterval> | undefined;
  private newestKnownOrderId = 0;

  constructor(
    private adminService: AdminService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.load();
    if (typeof document !== 'undefined') {
      this.refreshTimer = setInterval(() => {
        if (document.visibilityState === 'visible') {
          this.load(true);
        }
      }, 10000);
    }
  }

  ngOnDestroy(): void {
    if (this.refreshTimer) {
      clearInterval(this.refreshTimer);
    }
  }

  load(background = false): void {
    if (!background) {
      this.loading.set(true);
    }

    const requestedPage = background && this.pageNumber() > 1 ? 1 : this.pageNumber();
    this.adminService
      .getOrders({
        status: this.statusFilter() || undefined,
        search: this.search().trim() || undefined,
        fromDate: this.fromDate() || undefined,
        toDate: this.toDate() || undefined,
        pageNumber: requestedPage,
        pageSize: this.pageSize,
      })
      .subscribe({
        next: (result) => {
          const payload = result as any;
          const refreshedOrders = payload?.data ?? payload?.products ?? [];
          const refreshedTotal = payload?.totalCount ?? 0;
          const newestOrderId = refreshedOrders.reduce(
            (latest: number, order: AdminOrder) => Math.max(latest, order.id),
            0
          );
          const hasNewOrder = background && newestOrderId > this.newestKnownOrderId;

          if (!background || requestedPage === this.pageNumber() || hasNewOrder) {
            if (hasNewOrder && this.pageNumber() !== 1) {
              this.pageNumber.set(1);
            }
            this.orders.set(refreshedOrders);
          }

          this.totalCount.set(refreshedTotal);
          this.newestKnownOrderId = Math.max(this.newestKnownOrderId, newestOrderId);
          this.loading.set(false);
        },
        error: () => {
          if (!background) {
            this.loading.set(false);
            this.toastr.error('Could not load orders.', 'Orders');
          }
        },
      });
  }

  totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount() / this.pageSize));
  }

  onFilterChange(): void {
    this.pageNumber.set(1);
    this.load();
  }

  refresh(): void {
    this.load();
  }

  itemImage(image?: string): string {
    if (!image) return '/images/logo.png';
    if (image.startsWith('http') || image.startsWith('/')) return image;
    return `/Images/Products/${image.replace(/\\/g, '/')}`;
  }

  address(order: AdminOrder): string {
    const a = order.shippingAddress;
    if (!a) return '—';
    return [a.street, a.city, a.state, a.zipCode].filter(Boolean).join(', ') || '—';
  }

  searchTimeout: any;

  onSearchInput(value: string): void {
    this.search.set(value);
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => this.onFilterChange(), 400);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.pageNumber.set(page);
    this.load();
  }

  openDetails(order: AdminOrder): void {
    this.detailsLoading.set(true);
    this.adminService.getOrder(order.id).subscribe({
      next: (result) => {
        this.selectedOrder.set((result as any)?.data ?? order);
        this.detailsLoading.set(false);
      },
      error: () => {
        this.selectedOrder.set(order);
        this.detailsLoading.set(false);
        this.toastr.error('Could not load order details.', 'Orders');
      },
    });
  }

  closeDetails(): void {
    this.detailStatusOpen.set(false);
    this.selectedOrder.set(null);
  }

  toggleDetailStatus(): void {
    this.detailStatusOpen.update((open) => !open);
  }

  chooseDetailStatus(order: AdminOrder, status: string): void {
    this.detailStatusOpen.set(false);
    if (status !== order.status) {
      this.updateStatus(order, status);
    }
  }

  printInvoice(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    const cleanup = (): void => {
      document.body.classList.remove('is-printing-invoice');
      window.removeEventListener('afterprint', cleanup);
    };

    document.body.classList.add('is-printing-invoice');
    window.addEventListener('afterprint', cleanup, { once: true });
    window.print();
  }

  toggleStatusMenu(orderId: number): void {
    this.openStatusId.update((current) => current === orderId ? null : orderId);
  }

  toggleStatusFilter(): void {
    this.statusFilterOpen.update((open) => !open);
  }

  chooseStatusFilter(value: string): void {
    this.statusFilterOpen.set(false);
    this.statusFilter.set(value);
    this.onFilterChange();
  }

  statusClass(status: string): string {
    switch (status) {
      case 'PaymentRecevied':
        return 'status-chip status-chip--paid';
      case 'PaymentFaild':
        return 'status-chip status-chip--failed';
      case 'Shipped':
        return 'status-chip status-chip--shipped';
      case 'Delivered':
        return 'status-chip status-chip--delivered';
      default:
        return 'status-chip status-chip--pending';
    }
  }

  statusLabel(status: string): string {
    switch (status) {
      case 'PaymentRecevied':
        return 'Paid';
      case 'PaymentFaild':
        return 'Failed';
      case 'Shipped':
        return 'Shipped';
      case 'Delivered':
        return 'Delivered';
      default:
        return 'Pending';
    }

  }

  statusKey(status: string): string {
    return this.statusOptions.find((option) => option.value === status)?.labelKey ?? 'ADMIN_PENDING';
  }

  statusIcon(status: string): string {
    switch (status) {
      case 'PaymentRecevied': return 'check_circle';
      case 'PaymentFaild': return 'error';
      case 'Shipped': return 'local_shipping';
      case 'Delivered': return 'home';
      default: return 'schedule';
    }
  }

  paymentIcon(method?: string): string {
    if ((method ?? '').toLowerCase().includes('cod')) return 'payments';
    if ((method ?? '').toLowerCase().includes('stripe')) return 'credit_card';
    return 'account_balance_wallet';
  }

  changeStatus(order: AdminOrder, event: Event): void {
    const status = (event.target as HTMLSelectElement).value;
    this.updateStatus(order, status);
  }

  selectStatus(order: AdminOrder, status: string): void {
    this.openStatusId.set(null);
    if (status === order.status) return;
    this.updateStatus(order, status);
  }

  private updateStatus(order: AdminOrder, status: string): void {
    this.updatingId.set(order.id);
    this.adminService.updateOrderStatus(order.id, status).subscribe({
      next: () => {
        order.status = status;
        this.updatingId.set(null);
        this.toastr.success(`Order #${order.id} marked as ${this.statusLabel(status)}.`, 'Orders');
      },
      error: (error) => {
        this.updatingId.set(null);
        const message = error?.error?.message ?? 'Could not update the order status.';
        this.toastr.error(message, 'Orders');
      },
    });
  }

  money(value: number | null | undefined): string {
    return `$${(value ?? 0).toFixed(2)}`;
  }

  moneyInteger(value: number | null | undefined): string {
    return `$${(value ?? 0).toFixed(0)}`;
  }
}
