import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OrderService } from './core/Services/order.service';
import { BasketService } from './basket/basket.service';
import { TrackingStep } from './shared/components/order-tracking/order-tracking.component';
import { LanguageService } from './core/Services/language.service';
import * as QRCode from 'qrcode';

@Component({
  selector: 'app-order-detail-page',
  templateUrl: './order-detail-page.html',
  styleUrl: './order-detail-page.scss',
  standalone: false
})
export class OrderDetailPage implements OnInit {
  order = signal<any>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  trackingSteps = signal<TrackingStep[]>([]);
  qrCodeDataUrl = signal<string | null>(null);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService,
    public basketService: BasketService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) { this.router.navigate(['/my-order']); return; }

    this.orderService.getOrderByIdForUser(id).subscribe({
      next: (res) => {
        const o = (res as any)?.data ?? res;
        this.order.set(o);
        this.trackingSteps.set(this.buildSteps(o));
        void this.generateOrderQrCode(id);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load order details.');
        this.loading.set(false);
      }
    });
  }

  private async generateOrderQrCode(orderId: number): Promise<void> {
    if (typeof window === 'undefined') {
      return;
    }

    try {
      const trackingUrl = new URL(`/my-order/${orderId}`, window.location.origin).toString();
      this.qrCodeDataUrl.set(await QRCode.toDataURL(trackingUrl, {
        errorCorrectionLevel: 'M',
        margin: 2,
        width: 180,
        color: { dark: '#0f172a', light: '#ffffff' }
      }));
    } catch (error) {
      console.error('Failed to generate order QR code.', error);
      this.qrCodeDataUrl.set(null);
    }
  }

  formatStatus(status?: string): string {
    const keys: Record<string, string> = {
      'Pending': 'ADMIN_PENDING',
      'PaymentRecevied': 'ADMIN_PAID',
      'PaymentFaild': 'ADMIN_PAYMENT_FAILED',
      'Shipped': 'ADMIN_SHIPPED',
      'Delivered': 'ADMIN_DELIVERED',
    };
    const key = keys[status ?? ''];
    return key ? this.languageService.t(key) : status ?? this.languageService.t('ADMIN_PENDING');
  }

  private buildSteps(order: any): TrackingStep[] {
    const status: string = (order?.status ?? '').toLowerCase();
    // 'paymentrecevied' (sic — backend spelling) counts as paid.
    const placed   = true;
    const payment  = status !== 'pending';
    const shipped  = status === 'shipped'   || status === 'delivered';
    const delivered = status === 'delivered';

    const fmt = (d?: string) => d
      ? new Date(d).toLocaleString(
        this.languageService.currentLang() === 'ar' ? 'ar-EG' : 'en-US',
        { dateStyle: 'medium', timeStyle: 'short' }
      )
      : undefined;

    return [
      { label: 'Order Placed', labelKey: 'ORDER_STEP_PLACED',    date: fmt(order?.orderDate),     completed: placed,    current: !payment },
      { label: 'Payment',    labelKey: 'ORDER_STEP_PAYMENT',   date: fmt(order?.paymentDate),   completed: payment,   current: payment && !shipped },
      { label: 'Shipped',    labelKey: 'ORDER_STEP_SHIPPED',   date: fmt(order?.shippedDate),   completed: shipped,   current: shipped && !delivered },
      { label: 'Delivered',  labelKey: 'ORDER_STEP_DELIVERED', date: fmt(order?.deliveredDate), completed: delivered, current: false }
    ];
  }
}
