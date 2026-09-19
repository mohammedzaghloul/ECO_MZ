import { Component, Input } from '@angular/core';
import { LanguageService } from '../../../core/Services/language.service';

@Component({
  selector: 'app-status-badge',
  standalone: false,
  templateUrl: './status-badge.html',
  styleUrl: './status-badge.scss'
})
export class StatusBadgeComponent {
  @Input() status = '';

  constructor(private languageService: LanguageService) {}

  get className(): string {
    return `status-chip status-chip--${this.variant}`;
  }

  get label(): string {
    switch (this.status) {
      case 'Pending': return this.languageService.t('ADMIN_PENDING');
      case 'PaymentRecevied': return this.languageService.t('ADMIN_PAID');
      case 'PaymentFaild': return this.languageService.t('ADMIN_PAYMENT_FAILED');
      case 'Shipped': return this.languageService.t('ADMIN_SHIPPED');
      case 'Delivered': return this.languageService.t('ADMIN_DELIVERED');
      case 'Active': return this.languageService.t('ADMIN_COUPON_ACTIVE');
      case 'Inactive': return this.languageService.t('ADMIN_COUPON_INACTIVE');
      case 'Expired': return this.languageService.t('ADMIN_COUPON_EXPIRED');
      case 'Published': return this.languageService.t('ADMIN_PUBLISHED');
      case 'Draft': return this.languageService.t('ADMIN_DRAFT');
      case 'Admin': return this.languageService.t('ADMIN_ADMIN_ROLE');
      case 'Customer': return this.languageService.t('ADMIN_CUSTOMER_ROLE');
      default: return this.status || this.languageService.t('ADMIN_PENDING');
    }
  }

  private get variant(): string {
    switch (this.status) {
      case 'PaymentRecevied':
      case 'Active':
      case 'Published':
        return 'paid';
      case 'Admin':
        return 'admin';
      case 'PaymentFaild':
      case 'Expired':
        return 'failed';
      case 'Shipped':
      case 'Delivered':
        return 'delivered';
      case 'Inactive':
      case 'Draft':
      case 'Customer':
        return 'draft';
      default:
        return 'pending';
    }
  }
}
