import { Component, OnInit, signal } from '@angular/core';
import { BasketService } from '../basket.service';
import { LanguageService } from '../../core/Services/language.service';
import { OrderService } from '../../core/Services/order.service';
import { DeliveryMethodDto } from '../../shared/Models/api/order.models';

@Component({
  selector: 'app-basket',
  standalone: false,
  templateUrl: './basket.html',
  styleUrl: './basket.scss',
})
export class Basket implements OnInit {
  deliveryMethods = signal<DeliveryMethodDto[]>([]);

  constructor(
    public basketService: BasketService,
    public languageService: LanguageService,
    private orderService: OrderService
  ) {}

  ngOnInit(): void {
    // Real shipping prices come from the delivery methods table (same data used at checkout).
    this.orderService.getDeliveryMethods().subscribe({
      next: (response) => this.deliveryMethods.set(response?.data ?? []),
      error: () => this.deliveryMethods.set([]),
    });
  }

  get discountAmount(): number {
    return Math.min(this.basketService.discountAmount(), this.basketService.total());
  }

  /** The cheapest available delivery method — the exact amount picked at checkout. */
  get shippingCost(): number {
    const prices = this.deliveryMethods().map((method) => method.price);
    return prices.length ? Math.min(...prices) : 0;
  }

  get grandTotal(): number {
    return Math.max(0, this.basketService.total() - this.discountAmount + this.shippingCost);
  }
}
