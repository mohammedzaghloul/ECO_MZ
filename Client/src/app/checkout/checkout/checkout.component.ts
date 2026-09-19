import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BasketService } from '../../basket/basket.service';
import { OrderService } from '../../core/Services/order.service';
import { DeliveryMethodDto, OrderDto } from '../../shared/Models/api/order.models';
import { LanguageService } from '../../core/Services/language.service';
import { CheckoutLocation, CityShipping, LocationService } from '../../core/Services/location.service';
import { AccountService } from '../../core/Services/account.service';
import { AddressDto } from '../../shared/Models/api/account.models';
import { PaymentService } from '../../core/Services/payment.service';
import { environment } from '../../../environments/environment.development';
import { loadStripe, Stripe, StripeCardElement } from '@stripe/stripe-js';
import { of, switchMap } from 'rxjs';

export interface PaymentOption {
  id: string;
  labelKey: string;
  descKey: string;
  icon: string;
  brand: string;
  logo: string;
}

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss',
  standalone: false
})
export class CheckoutComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  deliveryMethods = signal<DeliveryMethodDto[]>([]);
  selectedDelivery = signal<DeliveryMethodDto | null>(null);
  selectedPaymentMethod = signal<string>('COD');
  stripeLoading = signal(false);
  stripeError = signal<string | null>(null);
  isSubmitting = signal(false);
  isSavingAddress = signal(false);
  error = signal<string | null>(null);
  currentStep = signal(1);
  governorates = signal<CheckoutLocation[]>([]);
  cityShipping = signal<CityShipping | null>(null);
  private savedAddress: AddressDto | null = null;
  private stripe: Stripe | null = null;
  private cardElement: StripeCardElement | null = null;

  readonly paymentOptions: PaymentOption[] = [
    { id: 'COD', labelKey: 'CHECKOUT_PAY_COD', descKey: 'CHECKOUT_PAY_COD_DESC', icon: 'local_shipping', brand: 'cod', logo: '' },
    { id: 'Stripe', labelKey: 'CHECKOUT_PAY_STRIPE', descKey: 'CHECKOUT_PAY_STRIPE_DESC', icon: 'credit_card', brand: 'stripe', logo: '/images/payment/visa.png' },
    { id: 'InstaPay', labelKey: 'CHECKOUT_PAY_INSTAPAY', descKey: 'CHECKOUT_PAY_INSTAPAY_DESC', icon: 'account_balance', brand: 'instapay', logo: '/images/payment/instapay.png' },
    { id: 'VodafoneCash', labelKey: 'CHECKOUT_PAY_VODAFONE', descKey: 'CHECKOUT_PAY_VODAFONE_DESC', icon: 'phone_android', brand: 'vodafone', logo: '/images/payment/vodafone-cash.png' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    public basketService: BasketService,
    private orderService: OrderService,
    private paymentService: PaymentService,
    private locationService: LocationService,
    private accountService: AccountService,
    public languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      fristName: ['', [Validators.required, Validators.minLength(2), Validators.pattern(/^[a-zA-Z\u0600-\u06FF\s]+$/)]],
      lastName:  ['', [Validators.required, Validators.minLength(2), Validators.pattern(/^[a-zA-Z\u0600-\u06FF\s]+$/)]],
      street:    ['', [Validators.required, Validators.minLength(5)]],
      city:      [{ value: '', disabled: true }, Validators.required],
      state:     ['', Validators.required],
      zipCode:   ['', [Validators.required, Validators.pattern(/^\d{4,10}$/)]]
    });
    this.form.get('state')?.valueChanges.subscribe(state => {
      const city = this.form.get('city');
      city?.reset('', { emitEvent: false });
      state ? city?.enable({ emitEvent: false }) : city?.disable({ emitEvent: false });
      this.cityShipping.set(null);
      this.selectedDelivery.set(null);
      this.error.set(null);
    });
    this.form.get('city')?.valueChanges.subscribe(cityValue => {
      const city = this.citiesForGovernorate().find(item => item.value === cityValue);
      this.cityShipping.set(null);
      this.selectedDelivery.set(null);
      this.error.set(null);
      if (city) {
        this.loadCityShipping(city.id);
      }
    });
    this.accountService.getAddress().subscribe({
      next: response => {
        this.savedAddress = response?.data ?? null;
        this.restoreSavedAddress();
      }
    });

    this.orderService.getDeliveryMethods().subscribe({
      next: (res) => {
        const methods = res?.data ?? (res as any) ?? [];
        this.deliveryMethods.set(Array.isArray(methods) ? methods : []);
        if (this.deliveryMethods().length && this.cityShipping()?.shippingAvailable
          && (this.basketService.basket()?.basketItems?.length ?? 0) > 0) {
          this.selectedDelivery.set(this.deliveryMethods()[0]);
        }
      },
      error: () => this.error.set('Failed to load delivery methods.')
    });
    this.locationService.getEgypt().subscribe({
      next: locations => {
        this.governorates.set(locations);
        this.restoreSavedAddress();
      },
      error: () => this.error.set('Failed to load locations.')
    });
  }

  private restoreSavedAddress(): void {
    const address = this.savedAddress;
    if (!address || !this.governorates().length) return;

    const governorate = this.governorates().find(location =>
      location.value === address.state || location.en === address.state || location.ar === address.state);
    if (!governorate) {
      console.warn('Saved address governorate not found in available locations:', address.state);
      this.error.set('Your saved address location is not available for shipping. Please select a new address.');
      return;
    }

    const city = governorate.cities.find(item =>
      item.value === address.city || item.en === address.city || item.ar === address.city);
    if (!city) {
      console.warn('Saved address city not found in governorate cities:', address.city);
      this.error.set('Your saved address city is not available for shipping. Please select a new city.');
      this.form.patchValue({
        fristName: address.firstName,
        lastName: address.lastName,
        street: address.street,
        state: governorate.value,
        zipCode: address.zipCode
      }, { emitEvent: false });
      this.form.get('city')?.enable({ emitEvent: false });
      return;
    }

    this.form.patchValue({
      fristName: address.firstName,
      lastName: address.lastName,
      street: address.street,
      state: governorate.value,
      city: city.value,
      zipCode: address.zipCode
    }, { emitEvent: false });
    this.form.get('city')?.enable({ emitEvent: false });
    this.loadCityShipping(city.id);
  }

  private loadCityShipping(cityId: number): void {
    this.cityShipping.set(null);
    this.selectedDelivery.set(null);
    this.error.set(null);
    this.locationService.getShipping(cityId).subscribe({
      next: shipping => {
        this.cityShipping.set(shipping);
        if (!shipping.shippingAvailable) {
          this.error.set('Shipping is unavailable for this city.');
          return;
        }
        if (this.deliveryMethods().length > 0) {
          this.selectedDelivery.set(this.deliveryMethods()[0]);
        }
      },
      error: (err) => {
        console.error('Shipping load error:', err);
        this.error.set('Shipping is unavailable for this city. Please select a different city.');
      }
    });
  }

  selectDelivery(method: DeliveryMethodDto): void {
    if (this.cityShipping()?.shippingAvailable) {
      this.selectedDelivery.set(method);
    }
  }

  citiesForGovernorate(): { id: number; value: string; en: string; ar: string; shippingPrice: number; deliveryDays: number; shippingAvailable: boolean }[] {
    return this.governorates().find(location => location.value === this.form?.get('state')?.value)?.cities ?? [];
  }

  localizedOption(option: { en: string; ar: string }): string {
    return this.languageService.currentLang() === 'ar' ? option.ar : option.en;
  }

  nextArrow(): string {
    return this.languageService.currentLang() === 'ar' ? '←' : '→';
  }

  backArrow(): string {
    return this.languageService.currentLang() === 'ar' ? '→' : '←';
  }

  nextStep(): void {
    if (this.currentStep() === 1) {
      if (this.form.invalid) {
        this.form.markAllAsTouched();
        return;
      }
      this.saveAddressAndContinue();
      return;
    } else if (this.currentStep() === 2 && (!this.selectedDelivery() || !this.cityShipping()?.shippingAvailable)) {
      this.error.set('Please select a delivery method.');
      return;
    }

    this.error.set(null);
    this.currentStep.update(step => Math.min(step + 1, 3));
  }

  private saveAddressAndContinue(): void {
    const values = this.form.getRawValue();
    const address: AddressDto = {
      firstName: values.fristName,
      lastName: values.lastName,
      street: values.street,
      city: values.city,
      state: values.state,
      zipCode: values.zipCode,
      country: 'Egypt'
    };

    if (this.isSameAsSavedAddress(address)) {
      this.currentStep.set(2);
      return;
    }

    this.isSavingAddress.set(true);
    this.error.set(null);
    this.accountService.updateAddress(address).subscribe({
      next: response => {
        this.savedAddress = response?.data ?? address;
        this.isSavingAddress.set(false);
        this.currentStep.set(2);
      },
      error: err => {
        console.error('Address save error:', err);
        this.error.set(err?.error?.message || 'Unable to save your address. Please try again.');
        this.isSavingAddress.set(false);
      }
    });
  }

  previousStep(): void {
    this.error.set(null);
    this.currentStep.update(step => Math.max(step - 1, 1));
  }

  get shippingCost(): number {
    return (this.basketService.basket()?.basketItems?.length ?? 0) > 0
      ? this.cityShipping()?.shippingPrice ?? 0
      : 0;
  }

  get orderTotal(): number {
    return this.basketService.total() + this.shippingCost;
  }

  ngOnDestroy(): void {
    if (this.cardElement) {
      this.cardElement.destroy();
      this.cardElement = null;
    }
  }

  async selectPaymentMethod(methodId: string): Promise<void> {
    this.selectedPaymentMethod.set(methodId);
    this.error.set(null);
    if (methodId === 'Stripe') {
      await this.setupStripe();
    }
  }

  async setupStripe(): Promise<void> {
    if (this.cardElement) return;
    this.stripeLoading.set(true);
    this.stripeError.set(null);
    try {
      if (!this.stripe) {
        this.stripe = await loadStripe(environment.stripePublishableKey);
      }
      if (!this.stripe) {
        this.stripeError.set('Unable to load Stripe payment.');
        return;
      }
      const elements = this.stripe.elements();
      this.cardElement = elements.create('card', {
        hidePostalCode: true,
        style: {
          base: {
            fontSize: '15px',
            color: '#1e293b',
            '::placeholder': { color: '#94a3b8' }
          }
        }
      });
      setTimeout(() => {
        const el = document.getElementById('card-element');
        if (el && this.cardElement) {
          this.cardElement.mount('#card-element');
        }
      }, 50);
    } catch (err: any) {
      console.error('Stripe setup error:', err);
      this.stripeError.set('Unable to initialize card input.');
    } finally {
      this.stripeLoading.set(false);
    }
  }

  async submit(): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const basket = this.basketService.basket();
    if (!basket?.id) { this.error.set('Your basket is empty.'); return; }
    if (!this.selectedDelivery() || !this.cityShipping()?.shippingAvailable) { this.error.set('Shipping is unavailable for this city.'); return; }

    this.isSubmitting.set(true);
    this.error.set(null);

    const rawAddress = this.form.getRawValue();
    const orderPayload: OrderDto = {
      basketId: basket.id,
      deliveryMethodId: this.selectedDelivery()!.id,
      governorateId: this.governorates().find(location => location.value === this.form.get('state')?.value)?.id ?? 0,
      cityId: this.citiesForGovernorate().find(city => city.value === this.form.get('city')?.value)?.id ?? 0,
      shippingAddressDto: {
        fristName: rawAddress.fristName,
        lastName: rawAddress.lastName,
        street: rawAddress.street,
        city: rawAddress.city,
        state: rawAddress.state,
        zipCode: rawAddress.zipCode,
        country: 'Egypt'
      },
      paymentMethod: this.selectedPaymentMethod()
    };

    const addressPayload: AddressDto = {
      firstName: rawAddress.fristName,
      lastName: rawAddress.lastName,
      street: rawAddress.street,
      city: rawAddress.city,
      state: rawAddress.state,
      zipCode: rawAddress.zipCode,
      country: 'Egypt'
    };

    const addressRequest = this.isSameAsSavedAddress(addressPayload)
      ? of(null)
      : this.accountService.updateAddress(addressPayload);

    if (this.selectedPaymentMethod() === 'Stripe') {
      if (!this.stripe || !this.cardElement) {
        await this.setupStripe();
      }
      this.paymentService.createOrUpdatePaymentIntent(basket.id, this.selectedDelivery()!.id).subscribe({
        next: async (updatedBasket) => {
          const clientSecret = updatedBasket.clientSecret;
          if (!clientSecret || !this.stripe || !this.cardElement) {
            this.error.set('Could not initialize card payment session.');
            this.isSubmitting.set(false);
            return;
          }

          const paymentResult = await this.stripe.confirmCardPayment(clientSecret, {
            payment_method: {
              card: this.cardElement,
              billing_details: {
                name: `${rawAddress.fristName} ${rawAddress.lastName}`.trim(),
                address: {
                  line1: rawAddress.street,
                  city: rawAddress.city,
                  state: rawAddress.state,
                  postal_code: rawAddress.zipCode,
                  country: 'EG'
                }
              }
            }
          });

          if (paymentResult.error) {
            this.error.set(paymentResult.error.message || 'Payment confirmation failed.');
            this.isSubmitting.set(false);
            return;
          }

          this.executeCreateOrder(addressRequest, orderPayload);
        },
        error: (err) => {
          console.error('Payment intent error:', err);
          this.error.set(err?.error?.message || err?.error?.detail || 'Could not process card payment.');
          this.isSubmitting.set(false);
        }
      });
      return;
    }

    this.executeCreateOrder(addressRequest, orderPayload);
  }

  private executeCreateOrder(addressRequest: any, orderPayload: OrderDto): void {
    addressRequest.pipe(
      switchMap(() => this.orderService.createOrder(orderPayload))
    ).subscribe({
      next: () => {
        this.basketService.clear();
        this.router.navigate(['/my-order']);
      },
      error: (err) => {
        console.error('Order creation error:', err);
        const msg = err?.error?.message || err?.error?.detail || err?.message || 'Order failed. Please try again.';
        this.error.set(msg);
        this.isSubmitting.set(false);
      }
    });
  }

  private isSameAsSavedAddress(address: AddressDto): boolean {
    if (!this.savedAddress) return false;
    return ['firstName', 'lastName', 'street', 'city', 'state', 'zipCode', 'country']
      .every(key => address[key as keyof AddressDto] === this.savedAddress![key as keyof AddressDto]);
  }

  isInvalid(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }
}