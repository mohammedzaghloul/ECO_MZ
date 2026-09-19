import { Component, ElementRef, OnInit, signal, ViewChild } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AdminDiscount, AdminService, SaveDiscount } from '../../core/Services/admin.service';

@Component({
  selector: 'app-admin-discounts',
  standalone: false,
  templateUrl: './discounts.html',
  styleUrl: './discounts.scss',
})
export class AdminDiscounts implements OnInit {
  @ViewChild('editorPanel') editorPanel?: ElementRef<HTMLElement>;

  discounts = signal<AdminDiscount[]>([]);
  loading = signal(true);
  saving = signal(false);

  editing = signal<SaveDiscount | null>(null);

  constructor(
    private adminService: AdminService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.adminService.getDiscounts().subscribe({
      next: (response) => {
        this.discounts.set(response?.data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toastr.error('Could not load coupons.', 'Coupons');
      },
    });
  }

  toNumber(value: string | null): number {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : 0;
  }

  startCreate(): void {
    this.editing.set({
      id: 0,
      code: '',
      isPercentage: true,
      value: 10,
      isActive: true,
      expiryDate: null,
      maxUses: null,
    });
    this.revealEditor();
  }

  startEdit(discount: AdminDiscount): void {
    this.editing.set({
      id: discount.id,
      code: discount.code,
      isPercentage: discount.isPercentage,
      value: discount.value,
      isActive: discount.isActive,
      expiryDate: discount.expiryDate ? discount.expiryDate.slice(0, 10) : null,
      maxUses: discount.maxUses,
    });
    this.revealEditor();
  }

  /** The editor renders above the list — bring it into view so the
      user actually sees it open (it is easy to miss when scrolled down). */
  private revealEditor(): void {
    setTimeout(() => {
      this.editorPanel?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
      this.editorPanel?.nativeElement.querySelector<HTMLInputElement>('input')?.focus();
    }, 0);
  }

  cancelEdit(): void {
    this.editing.set(null);
  }

  save(): void {
    const form = this.editing();
    if (!form) return;

    if (!form.code.trim()) {
      this.toastr.warning('Please enter a coupon code.', 'Coupons');
      return;
    }
    if (!form.value || form.value <= 0) {
      this.toastr.warning('Please enter a discount value.', 'Coupons');
      return;
    }

    this.saving.set(true);
    this.adminService
      .saveDiscount({
        ...form,
        code: form.code.trim().toUpperCase(),
        expiryDate: form.expiryDate ? new Date(form.expiryDate).toISOString() : null,
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.editing.set(null);
          this.toastr.success('Coupon saved.', 'Coupons');
          this.load();
        },
        error: (error) => {
          this.saving.set(false);
          const message = error?.error?.message ?? 'Could not save the coupon.';
          this.toastr.error(message, 'Coupons');
        },
      });
  }

  remove(discount: AdminDiscount): void {
    if (!confirm(`Delete coupon "${discount.code}"?`)) return;

    this.adminService.deleteDiscount(discount.id).subscribe({
      next: () => {
        this.toastr.success('Coupon deleted.', 'Coupons');
        this.load();
      },
      error: () => this.toastr.error('Could not delete the coupon.', 'Coupons'),
    });
  }

  valueLabel(discount: AdminDiscount): string {
    return discount.isPercentage ? `${discount.value}% off` : `$${discount.value.toFixed(2)} off`;
  }

  isExpired(discount: AdminDiscount): boolean {
    return !!discount.expiryDate && new Date(discount.expiryDate) < new Date();
  }

  usageLabel(discount: AdminDiscount): string {
    return discount.maxUses ? `${discount.usedCount} / ${discount.maxUses}` : `${discount.usedCount}`;
  }
}
