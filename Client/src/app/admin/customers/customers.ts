import { Component, OnInit, signal } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AdminCustomer, AdminCustomerDetail, AdminService } from '../../core/Services/admin.service';
import { LanguageService } from '../../core/Services/language.service';

@Component({
  selector: 'app-admin-customers',
  standalone: false,
  templateUrl: './customers.html',
  styleUrl: './customers.scss',
})
export class AdminCustomers implements OnInit {
  customers = signal<AdminCustomer[]>([]);
  totalCount = signal(0);
  pageNumber = signal(1);
  pageSize = 10;
  search = signal('');
  roleFilter = signal('all');
  loading = signal(true);
  togglingId = signal<string | null>(null);
  pendingAdminRemoval = signal<string | null>(null);
  pendingDeleteId = signal<string | null>(null);
  deletingId = signal<string | null>(null);

  readonly roleOptions = [
    { value: 'all', labelKey: 'ADMIN_CUSTOMER_FILTER_ALL', icon: 'groups' },
    { value: 'customer', labelKey: 'ADMIN_CUSTOMER_FILTER_CUSTOMERS', icon: 'person' },
    { value: 'admin', labelKey: 'ADMIN_CUSTOMER_FILTER_ADMINS', icon: 'admin_panel_settings' },
  ];

  selectedCustomer = signal<AdminCustomerDetail | null>(null);
  detailLoading = signal(false);

  constructor(
    private adminService: AdminService,
    private toastr: ToastrService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.adminService
      .getCustomers({
        search: this.search().trim() || undefined,
        role: this.roleFilter() === 'all' ? undefined : this.roleFilter(),
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize,
      })
      .subscribe({
        next: (result) => {
          const payload = result as any;
          this.customers.set(payload?.data ?? payload?.products ?? []);
          this.totalCount.set(payload?.totalCount ?? 0);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.toast('ADMIN_CUSTOMERS_LOAD_ERROR', 'ADMIN_CUSTOMERS_TITLE', 'error');
        },
      });
  }

  refresh(): void {
    this.load();
  }

  totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount() / this.pageSize));
  }

  searchTimeout: any;

  onSearchInput(value: string): void {
    this.search.set(value);
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.pageNumber.set(1);
      this.load();
    }, 400);
  }

  clearSearch(): void {
    if (!this.search()) return;
    this.search.set('');
    clearTimeout(this.searchTimeout);
    this.pageNumber.set(1);
    this.load();
  }

  onRoleFilter(value: string): void {
    if (this.roleFilter() === value) return;
    this.roleFilter.set(value);
    this.pageNumber.set(1);
    this.load();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.pageNumber.set(page);
    this.load();
  }

  view(customer: AdminCustomer): void {
    this.detailLoading.set(true);
    this.adminService.getCustomer(customer.id).subscribe({
      next: (response) => {
        this.selectedCustomer.set(response?.data ?? null);
        this.detailLoading.set(false);
      },
      error: () => {
        this.detailLoading.set(false);
        this.toast('ADMIN_CUSTOMER_DETAILS_ERROR', 'ADMIN_CUSTOMERS_TITLE', 'error');
      },
    });
  }

  isAdmin(customer: AdminCustomer): boolean {
    return customer.roles.includes('Admin');
  }

  toggleAdmin(customer: AdminCustomer): void {
    const grant = !this.isAdmin(customer);
    if (!grant && this.pendingAdminRemoval() !== customer.id) {
      this.pendingAdminRemoval.set(customer.id);
      return;
    }
    this.pendingAdminRemoval.set(null);

    this.togglingId.set(customer.id);
    this.adminService.setAdminRole(customer.id, grant).subscribe({
      next: () => {
        customer.roles = grant ? [...customer.roles, 'Admin'] : customer.roles.filter((r) => r !== 'Admin');
        this.togglingId.set(null);
        this.toast(grant ? 'ADMIN_ROLE_GRANTED' : 'ADMIN_ROLE_REMOVED', 'ADMIN_CUSTOMERS_TITLE', 'success', customer.email);
      },
      error: () => {
        this.togglingId.set(null);
        this.toast('ADMIN_ROLE_UPDATE_ERROR', 'ADMIN_CUSTOMERS_TITLE', 'error');
      },
    });
  }

  deleteCustomer(customer: AdminCustomer): void {
    if (this.isAdmin(customer)) return;
    if (this.pendingDeleteId() !== customer.id) {
      this.pendingDeleteId.set(customer.id);
      return;
    }

    this.pendingDeleteId.set(null);
    this.deletingId.set(customer.id);
    this.adminService.deleteCustomer(customer.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.selectedCustomer.set(null);
        this.toast('ADMIN_CUSTOMER_DELETED', 'ADMIN_CUSTOMERS_TITLE', 'success');
        this.load();
      },
      error: (error) => {
        this.deletingId.set(null);
        this.toast(error?.error?.message ?? 'ADMIN_CUSTOMER_DELETE_ERROR', 'ADMIN_CUSTOMERS_TITLE', 'error');
      },
    });
  }

  money(value: number | null | undefined): string {
    return `$${(value ?? 0).toFixed(2)}`;
  }

  private toast(key: string, titleKey: string, type: 'success' | 'error', email?: string): void {
    const arabic = this.languageService.currentLang() === 'ar';
    const messages: Record<string, [string, string]> = {
      ADMIN_CUSTOMERS_LOAD_ERROR: ['تعذر تحميل العملاء.', 'Could not load customers.'],
      ADMIN_CUSTOMER_DETAILS_ERROR: ['تعذر تحميل تفاصيل العميل.', 'Could not load customer details.'],
      ADMIN_ROLE_GRANTED: [`تم منح صلاحية المسؤول لـ ${email ?? ''}.`, `${email ?? ''} is now an admin.`],
      ADMIN_ROLE_REMOVED: [`تمت إزالة صلاحية المسؤول من ${email ?? ''}.`, `Admin access removed from ${email ?? ''}.`],
      ADMIN_ROLE_UPDATE_ERROR: ['تعذر تحديث صلاحية المسؤول.', 'Could not update the admin role.'],
      ADMIN_CUSTOMER_DELETED: ['تم حذف المستخدم بنجاح.', 'Customer deleted successfully.'],
      ADMIN_CUSTOMER_DELETE_ERROR: ['تعذر حذف المستخدم.', 'Could not delete the customer.'],
    };
    const message = messages[key] ?? ['', ''];
    this.toastr[type](arabic ? message[0] : message[1], arabic ? 'العملاء' : 'Customers');
  }

}
