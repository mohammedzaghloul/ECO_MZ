import { Component, OnInit, signal, Inject, PLATFORM_ID } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import {
  LandingService,
  LandingSectionContent,
} from './core/Services/landing.service';
import { ProductService } from './core/Services/product.service';
import { BasketService } from './basket/basket.service';
import { ShopService } from './shop/shop.service';
import { IProduct } from './shared/Models/product';
import { ToastrService } from 'ngx-toastr';
import { LocationService, CheckoutLocation } from './core/Services/location.service';
import { GuestOrderRequest } from './core/Services/landing.service';
import { LanguageService } from './core/Services/language.service';
import { OfflineSupportService } from './core/Services/offline-support.service';

interface RenderSection {
  type: string;
  title: string;
  imageUrl: string;
  content: LandingSectionContent;
}

@Component({
  selector: 'app-landing-view-page',
  standalone: false,
  templateUrl: './landing-view-page.html',
  styleUrl: './landing-view-page.scss',
})
export class LandingViewPage implements OnInit {
  pageTitle = signal('');
  sections = signal<RenderSection[]>([]);
  loading = signal(true);
  notFound = signal(false);
  product = signal<IProduct | null>(null);
  quantity = signal(1);
  landingPageId = signal(0);
  locations = signal<CheckoutLocation[]>([]);
  orderSubmitting = signal(false);
  orderSuccess = signal('');
  private analyticsSessionId = '';
  private formStarted = false;
  private visitTracked = false;
  guestOrder = { customerName: '', phone: '', governorateId: 0, cityId: 0, address: '', notes: '' };
  activePhotoIndex = signal(0);
  hoveredPhotoIndex = signal<number | null>(null);
  mediaMode = signal<'photo' | 'video'>('photo');
  selectedSize = signal('');
  showSizeGuide = signal(false);
  showNotes = signal(false);
  landingTemplate = signal('minimal');
  accentColor = signal('#2d5a43');
  fontFamily = signal('system');
  videoUrl = signal('');
  whatsAppUrl = signal('');
  readonly brandName = signal('متجرك');
  readonly brandLogoUrl = signal('/images/logo.png');
  readonly currentYear = new Date().getFullYear();

  readonly placeholderSvg = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(`
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 600 600" width="100%" height="100%">
      <defs>
        <linearGradient id="bgGrad" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stop-color="#f8fafc"/>
          <stop offset="100%" stop-color="#f1f5f9"/>
        </linearGradient>
      </defs>
      <rect width="600" height="600" rx="32" fill="url(#bgGrad)"/>
      <circle cx="300" cy="270" r="75" fill="#ffffff" opacity="0.9"/>
      <g fill="#2563eb" transform="translate(268, 238) scale(2.6)">
        <path d="M19 6h-2c0-2.76-2.24-5-5-5S7 3.24 7 6H5c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2zm-7-3c1.66 0 3 1.34 3 3H9c0-1.66 1.34-3 3-3zm7 17H5V8h14v12zm-7-8c-1.66 0-3-1.34-3-3H7c0 2.76 2.24 5 5 5s5-2.24 5-5h-2c0 1.66-1.34 3-3 3z"/>
      </g>
      <text x="300" y="390" fill="#0f172a" font-family="system-ui, -apple-system, sans-serif" font-size="22" text-anchor="middle" font-weight="700">منتج مميز وحصري</text>
      <text x="300" y="422" fill="#64748b" font-family="system-ui, -apple-system, sans-serif" font-size="15" text-anchor="middle" font-weight="500">جودة عالية وضمان استرجاع</text>
    </svg>
  `);

  onImageError(event: Event): void {
    const target = event.target as HTMLImageElement;
    if (target && !target.dataset['fallback']) {
      target.dataset['fallback'] = 'true';
      target.src = this.placeholderSvg;
    }
  }


  constructor(
    private route: ActivatedRoute,
    private landingService: LandingService,
    private productService: ProductService,
    private basketService: BasketService,
    private shopService: ShopService,
    private toastr: ToastrService,
    private locationService: LocationService,
    private languageService: LanguageService,
    private offlineSupport: OfflineSupportService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  ngOnInit(): void {
    this.locationService.getEgypt().subscribe({
      next: (locations) => this.locations.set(locations),
      error: () => undefined,
    });
    this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug');
      if (!slug) {
        this.notFound.set(true);
        this.loading.set(false);
        return;
      }
      this.load(slug);
    });
  }

  private load(slug: string): void {
    this.loading.set(true);
    this.landingService.getBySlug(slug).subscribe({
      next: (response) => {
        const page = response?.data;
        if (!page) {
          this.notFound.set(true);
          this.loading.set(false);
          return;
        }

        this.pageTitle.set(page.title);
        this.landingPageId.set(page.id);
        this.landingTemplate.set(page.template || 'minimal');
        // Use sage green if color is missing or default indigo
        const color = (!page.accentColor || page.accentColor === '#4f46e5') ? '#2d5a43' : page.accentColor;
        this.accentColor.set(color);
        this.fontFamily.set(page.fontFamily || 'system');
        this.videoUrl.set(page.videoUrl || '');
        if (page.whatsAppNumber) {
          const message = encodeURIComponent(page.whatsAppMessage || `أريد طلب ${page.title}`);
          this.whatsAppUrl.set(`https://wa.me/${page.whatsAppNumber.replace(/\D/g, '')}?text=${message}`);
        }
        this.trackLandingEvent('visit');
        if (isPlatformBrowser(this.platformId)) {
          document.title = page.title;
        }

        const renderSections = page.sections
          .filter((section) => section.isVisible)
          .sort((a, b) => a.sortOrder - b.sortOrder)
          .map((section) => ({
            type: String(section.sectionType),
            title: section.title,
            imageUrl: section.imageUrl,
            content: LandingService.parseContent(section),
          }));
        const heroHeadline = renderSections.find(section => section.type === 'hero')?.content.headline;
        const seenTypes = new Set<string>();
        const deduplicatedSections = renderSections.filter(section => {
          if (section.type === 'showcase' && heroHeadline && section.content.headline === heroHeadline) return false;
          if (section.type === 'cta' && renderSections.some(item => item.type === 'orderform')) return false;
          if (seenTypes.has(section.type)) return false;
          seenTypes.add(section.type);
          return true;
        });
        const sectionPriority: Record<string, number> = {
          hero: 0,
          trustbar: 1,
          features: 2,
          showcase: 3,
          reviews: 4,
          faq: 5,
          orderform: 6,
          cta: 7,
        };
        this.sections.set(
          deduplicatedSections.sort((a, b) =>
            (sectionPriority[a.type] ?? 99) - (sectionPriority[b.type] ?? 99))
        );

        if (page.productId) {
          this.loadProduct(page.productId);
        } else if (renderSections.length === 0) {
          this.sections.set([this.defaultSection('hero', null)]);
        }

        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  availableCities(): CheckoutLocation['cities'] {
    return this.locations().find((location) => location.id === Number(this.guestOrder.governorateId))?.cities ?? [];
  }

  submitGuestOrder(): void {
    const product = this.product();
    const rawPhone = (this.guestOrder.phone || '').trim();
    // Normalize phone (strip spaces, dashes, country code +20)
    const cleanPhone = rawPhone.replace(/[\s\-()]/g, '').replace(/^(\+20|0020|20)/, '0');
    const phonePattern = /^01[0125][0-9]{8}$/;

    if (!this.guestOrder.customerName.trim()) {
      this.toastr.warning('يرجى إدخال الاسم بالكامل.', 'بيانات الطلب');
      return;
    }
    if (!phonePattern.test(cleanPhone)) {
      this.toastr.warning('يرجى إدخال رقم موبايل مصري صحيح (مثال: 01012345678).', 'رقم الموبايل');
      return;
    }

    if (product) {
      if (this.isClothing() && !this.selectedSize()) {
        this.toastr.warning('اختار المقاس أولاً.', 'بيانات المنتج');
        return;
      }
      if (!this.guestOrder.governorateId || !this.guestOrder.cityId || this.guestOrder.address.trim().length < 5) {
        this.toastr.warning('يرجى اختيار المحافظة والمدينة وكتابة العنوان بالتفصيل.', 'بيانات التوصيل');
        return;
      }

      this.orderSubmitting.set(true);
      const request: GuestOrderRequest = {
        landingPageId: this.landingPageId(),
        productId: product.id,
        quantity: this.quantity(),
        selectedSize: this.selectedSize() || undefined,
        customerName: this.guestOrder.customerName.trim(),
        phone: cleanPhone,
        governorateId: Number(this.guestOrder.governorateId),
        cityId: Number(this.guestOrder.cityId),
        address: this.guestOrder.address.trim(),
        notes: this.guestOrder.notes.trim() || undefined,
      };
      this.landingService.createGuestOrder(request).subscribe({
        next: (response) => {
          this.orderSuccess.set(response?.data?.trackingCode ?? 'CONFIRMED-' + Date.now().toString().slice(-6));
          this.orderSubmitting.set(false);
          this.trackLandingEvent('order_submitted');
        },
        error: (error) => {
          this.orderSubmitting.set(false);
          if (error?.status === 0 || error?.status >= 500 || (typeof navigator !== 'undefined' && !navigator.onLine)) {
            const key = typeof crypto?.randomUUID === 'function' ? crypto.randomUUID() : String(Date.now());
            void this.offlineSupport.queueGuestOrder(request, key).then(() => {
              this.toastr.info(this.offlineSupport.describeFailure(error), 'سيتم إرسال الطلب تلقائيًا');
            });
            return;
          }
          this.toastr.error(error?.error?.message ?? 'تعذر إنشاء الطلب.', 'الطلب');
        },
      });
    } else {
      // Landing page does NOT sell a product (Inquiry / Lead mode)
      this.orderSubmitting.set(true);
      setTimeout(() => {
        this.orderSubmitting.set(false);
        this.orderSuccess.set('INQUIRY-' + Date.now().toString().slice(-6));
        this.trackLandingEvent('order_submitted');
        this.toastr.success('تم استلام استفسارك بنجاح! سنتواصل معك بأقرب وقت.', 'تم بنجاح');
      }, 600);
    }
  }

  trackFormStarted(): void {
    if (this.formStarted) return;
    this.formStarted = true;
    this.locationService.getEgypt().subscribe({
      next: (locations) => this.locations.set(locations),
      error: () => this.toastr.error('تعذر تحميل المحافظات، حاول مرة أخرى.', 'بيانات التوصيل'),
    });
    this.trackLandingEvent('form_started');
  }

  private trackLandingEvent(eventType: 'visit' | 'form_started' | 'order_submitted'): void {
    if (!isPlatformBrowser(this.platformId) || !this.landingPageId()) return;
    if (!this.analyticsSessionId) {
      const storageKey = 'eco-landing-session-id';
      this.analyticsSessionId = sessionStorage.getItem(storageKey) ?? crypto.randomUUID();
      sessionStorage.setItem(storageKey, this.analyticsSessionId);
    }
    if (eventType === 'visit') {
      if (this.visitTracked) return;
      this.visitTracked = true;
    }
    const params = new URLSearchParams(window.location.search);
    const source = params.get('utm_source')?.trim().toLowerCase() || 'direct';
    this.landingService.trackEvent({
      landingPageId: this.landingPageId(),
      sessionId: this.analyticsSessionId,
      eventType,
      source,
    }).subscribe({ error: () => undefined });
  }

  private loadProduct(productId: number): void {
    this.productService.getById(productId).subscribe({
      next: (response) => {
        const data = (response as any)?.data ?? null;
        this.product.set(data);
        this.activePhotoIndex.set(0);
        const heroHeadline = this.sections().find(section => section.type === 'hero')?.content.headline;
        let current = this.sections();
        if (heroHeadline && data) {
          current = current.filter(section =>
            !(section.type === 'showcase' && section.content.headline === heroHeadline)
          );
        }
        if (current.length === 0 && data) {
          current = [
            this.defaultSection('hero', data),
            this.defaultSection('features', data),
            this.defaultSection('orderform', data),
          ];
        } else if (data) {
          const hasFeatures = current.some(s => s.type === 'features');
          const hasOrderForm = current.some(s => s.type === 'orderform');
          if (!hasFeatures) {
            current.push(this.defaultSection('features', data));
          }
          if (!hasOrderForm) {
            current.push(this.defaultSection('orderform', data));
          }
          if (!current.some(s => s.type === 'trustbar')) {
            current.push(this.defaultSection('trustbar', data));
          }
          if (!current.some(s => s.type === 'reviews')) {
            current.push(this.defaultSection('reviews', data));
          }
          if (!current.some(s => s.type === 'faq')) {
            current.push(this.defaultSection('faq', data));
          }
        }
        this.sections.set(this.orderSections(current));
      },
      error: () => this.product.set(null),
    });
  }

  private defaultSection(type: string, product: IProduct | null): RenderSection {
    const name = product?.name ?? this.pageTitle();
    const description = product?.description ?? '';
    const image = product?.photos?.[0] ?? '';

    if (type === 'trustbar') {
      return {
        type,
        title: '',
        imageUrl: '',
        content: {
          items: [
            { icon: 'verified', title: 'منتج موثوق' },
            { icon: 'local_shipping', title: 'شحن سريع' },
            { icon: 'support_agent', title: 'دعم متواصل' },
          ],
        },
      };
    }

    if (type === 'features') {
      return {
        type,
        title: 'لماذا ستختاره؟',
        imageUrl: '',
        content: {
          items: [
            { icon: 'workspace_premium', title: 'جودة مميزة', text: 'تفاصيل مصممة لتقدم لك تجربة أفضل كل يوم.' },
            { icon: 'favorite', title: 'اختيار ذكي', text: 'قيمة حقيقية ومواصفات تناسب احتياجك.' },
            { icon: 'shopping_bag', title: 'اطلب بسهولة', text: 'خطوات بسيطة وسريعة من الصفحة مباشرة.' },
          ],
        },
      };
    }

    if (type === 'cta') {
      return {
        type,
        title: '',
        imageUrl: '',
        content: {
          headline: 'جاهز تخلي المنتج ده جزء من يومك؟',
          body: 'اطلب الآن واستمتع بتجربة شراء سهلة وآمنة.',
          buttonText: 'ابدأ طلبك الآن',
          buttonLink: '#order',
        },
      };
    }

    if (type === 'reviews') {
      return {
        type,
        title: 'آراء العملاء',
        imageUrl: '',
        content: {
          items: [
            { name: 'سارة محمد', rating: 5, text: 'الخامة ممتازة ووصل المنتج بسرعة والتغليف كان محترم.' },
            { name: 'محمود علي', rating: 5, text: 'طلبت بسهولة ووصل المنتج في معاده. تجربة مريحة جدًا.' },
            { name: 'نور أحمد', rating: 4, text: 'منتج عملي والجودة واضحة من أول استخدام.' },
          ],
        },
      };
    }

    if (type === 'faq') {
      return {
        type,
        title: 'أسئلة قبل الطلب',
        imageUrl: '',
        content: {
          items: [
            { question: 'هل يمكن معاينة المنتج قبل الدفع؟', answer: 'نعم، يمكنك معاينة المنتج عند الاستلام قبل الدفع.' },
            { question: 'متى يصل الطلب؟', answer: 'عادة يصل خلال 2 إلى 4 أيام عمل حسب المحافظة.' },
            { question: 'هل يوجد استبدال أو استرجاع؟', answer: 'نعم، نوفر استبدالًا أو استرجاعًا خلال 14 يومًا حسب سياسة المتجر.' },
          ],
        },
      };
    }

    if (type === 'orderform') {
      return {
        type,
        title: 'اطلب المنتج الآن',
        imageUrl: '',
        content: {
          headline: name,
          body: 'أكمل بيانات الطلب وسنتواصل معك لتأكيده.',
          buttonText: 'إضافة إلى السلة',
          buttonLink: '',
        },
      };
    }

    return {
      type,
      title: type === 'showcase' ? 'المنتج' : name,
      imageUrl: image,
      content: {
        headline: name,
        body: type === 'hero'
          ? 'اختيار عملي بجودة تستحقها. اكتشف التفاصيل واطلبه الآن بسهولة.'
          : description || 'كل ما تحتاجه في منتج واحد، بتصميم أنيق وتجربة استخدام سهلة.',
        buttonText: 'اطلب الآن',
        buttonLink: '#order',
      },
    };
  }

  private orderSections(sections: RenderSection[]): RenderSection[] {
    const priority: Record<string, number> = {
      hero: 0,
      trustbar: 1,
      showcase: 2,
      features: 3,
      reviews: 4,
      faq: 5,
      cta: 6,
      // The order form always renders last: the customer sees benefits,
      // guarantees, reviews and FAQ before being asked for delivery details.
      orderform: 7,
    };
    return [...sections].sort((a, b) => (priority[a.type] ?? 99) - (priority[b.type] ?? 99));
  }

  isInternalLink(link?: string): boolean {
    return !!link && link.startsWith('/');
  }

  sectionImage(section: RenderSection): string {
    const url = section.imageUrl || (this.product() ? this.productPhotos()[0] : '');
    return this.resolveImage(url);
  }

  // Avoid repeating the hero photo: when a showcase section points at the
  // same main image, show the next product photo instead.
  showcaseImage(section: RenderSection): string {
    const photos = this.productPhotos();
    const fallback = this.sectionImage(section);
    if (photos.length > 1 && fallback === this.resolveImage(photos[0])) {
      return this.resolveImage(photos[1]);
    }
    return fallback;
  }

  productPhotos(): string[] {
    const photos = this.product()?.photos;
    return Array.isArray(photos) ? photos : photos ? [photos as any] : [];
  }

  isClothing(): boolean {
    const product = this.product();
    const category = `${product?.categoryName ?? ''} ${product?.name ?? ''}`.toLowerCase();
    return /ملابس|أزياء|فساتين|تيشيرت|قميص|بنطلون|clothing|fashion|shirt|dress|pants/.test(category) ||
      (product?.specifications ?? []).some(item => /مقاس|size/i.test(item.label));
  }

  clothingSizes(): string[] {
    const sizeSpec = this.product()?.specifications?.find(item => /مقاس|size/i.test(item.label));
    return sizeSpec?.value
      ?.split(/[,،|/]/)
      .map(size => size.trim())
      .filter(Boolean) ?? ['S', 'M', 'L', 'XL', 'XXL'];
  }

  sizeStock(size: string): number | null {
    const specification = this.product()?.specifications?.find(item => /مخزون.*مقاس|size.*stock/i.test(item.label));
    if (!specification) return null;
    try {
      const stock = JSON.parse(specification.value) as Record<string, number>;
      const value = stock[size];
      return Number.isFinite(value) ? value : null;
    } catch {
      return null;
    }
  }

  displayPhoto(index: number): string {
    const photoIndex = this.hoveredPhotoIndex() ?? index;
    return this.resolveImage(this.productPhotos()[photoIndex] ?? this.productPhotos()[index]);
  }

  activeProductPhoto(): string {
    const photos = this.productPhotos();
    return photos[this.activePhotoIndex()] ?? photos[0] ?? '';
  }

  selectPhoto(index: number): void {
    const photos = this.productPhotos();
    if (index >= 0 && index < photos.length) {
      this.activePhotoIndex.set(index);
    }
  }

  previousPhoto(): void {
    const photos = this.productPhotos();
    if (photos.length < 2) return;
    this.activePhotoIndex.update((index) => (index - 1 + photos.length) % photos.length);
  }

  nextPhoto(): void {
    const photos = this.productPhotos();
    if (photos.length < 2) return;
    this.activePhotoIndex.update((index) => (index + 1) % photos.length);
  }

  resolveImage(url?: string): string {
    if (!url) return this.placeholderSvg;
    if (url.startsWith('http') || url.startsWith('data:')) return url;

    const baseUrl = this.shopService.BaseUrl().replace('api/', '');
    const normalised = url.replace(/\\/g, '/');

    if (normalised.includes('Images/')) {
      return `${baseUrl}${normalised}`;
    }
    return `${baseUrl}Images/Products/${normalised}`;
  }

  addToBasket(): void {
    const product = this.product();
    if (!product) return;

    this.basketService.add(product, this.quantity());
    if (isPlatformBrowser(this.platformId)) {
      this.toastr.success(
        this.languageService.t('CART_ADDED_MESSAGE', { qty: this.quantity(), name: product.name }),
        this.languageService.t('CART_UPDATED'),
        {
          timeOut: 1000,
          closeButton: false,
          progressBar: false,
        }
      );
    }
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

  orderTotal(): number {
    const p = this.product();
    return p ? p.newPrice * this.quantity() : 0;
  }

  fontStack(): string {
    switch (this.fontFamily()) {
      case 'cairo':
        return "'Cairo', 'Tajawal', system-ui, sans-serif";
      case 'tajawal':
        return "'Tajawal', 'Cairo', system-ui, sans-serif";
      case 'inter':
        return "'Inter', system-ui, sans-serif";
      default:
        return "'Cairo', 'Tajawal', system-ui, -apple-system, sans-serif";
    }
  }

  isArabic(): boolean {
    return this.languageService.currentLang() === 'ar';
  }

  toggleLanguage(): void {
    this.languageService.toggleLanguage();
  }

  text(arabic: string, english: string): string {
    return this.isArabic() ? arabic : english;
  }

  scrollToOrderForm(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const formEl = document.getElementById('order') || document.getElementById('orderform') || document.querySelector('.lp-order');
    if (formEl) {
      formEl.scrollIntoView({ behavior: 'smooth', block: 'start' });
      this.trackFormStarted();
    }
  }

  scrollToSection(id: string): void {
    if (!isPlatformBrowser(this.platformId)) return;
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  getInitials(name: string): string {
    if (!name) return '؟';
    const parts = name.trim().split(' ');
    if (parts.length === 1) {
      return parts[0].charAt(0).toUpperCase();
    }
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  }

  getAvatarColor(index: number): string {
    const colors = [
      '#2d5a43', // Primary green
      '#3b82f6', // Blue
      '#8b5cf6', // Purple
      '#ec4899', // Pink
      '#f59e0b', // Orange
      '#10b981', // Emerald
    ];
    return colors[index % colors.length];
  }
}
