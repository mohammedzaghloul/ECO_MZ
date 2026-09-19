import { Component, OnInit, signal, Inject, PLATFORM_ID } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import {
  LandingSectionContent,
  LandingSectionType,
  LandingService,
  SaveLandingPageDto,
  LandingSectionItem,
} from '../../core/Services/landing.service';
import { ProductService } from '../../core/Services/product.service';
import { ProductDto } from '../../shared/Models/api/product.models';

interface EditorSection {
  sectionType: LandingSectionType | string;
  title: string;
  isVisible: boolean;
  imageUrl: string;
  content: LandingSectionContent;
}

interface SectionDef {
  type: LandingSectionType;
  label: string;
  icon: string;
  hasImage: boolean;
  hasHeadline: boolean;
  hasBody: boolean;
  hasButton: boolean;
  itemsKind: 'icon' | 'review' | 'faq' | null;
  itemsLabel: string;
}

@Component({
  selector: 'app-admin-landing-editor',
  standalone: false,
  templateUrl: './landing-editor.html',
  styleUrl: './landing-editor.scss',
})
export class AdminLandingEditor implements OnInit {
  readonly sectionDefs: SectionDef[] = [
    { type: 'hero', label: 'ADMIN_BLOCK_HERO', icon: 'wallpaper', hasImage: true, hasHeadline: true, hasBody: true, hasButton: true, itemsKind: null, itemsLabel: '' },
    { type: 'trustbar', label: 'ADMIN_BLOCK_TRUST_BAR', icon: 'verified', hasImage: false, hasHeadline: false, hasBody: false, hasButton: false, itemsKind: 'icon', itemsLabel: 'ADMIN_TRUST_BADGES' },
    { type: 'features', label: 'ADMIN_BLOCK_FEATURES', icon: 'stars', hasImage: false, hasHeadline: false, hasBody: false, hasButton: false, itemsKind: 'icon', itemsLabel: 'ADMIN_FEATURE_CARDS' },
    { type: 'showcase', label: 'ADMIN_BLOCK_SHOWCASE', icon: 'inventory_2', hasImage: true, hasHeadline: true, hasBody: true, hasButton: true, itemsKind: null, itemsLabel: '' },
    { type: 'reviews', label: 'ADMIN_BLOCK_REVIEWS', icon: 'reviews', hasImage: false, hasHeadline: false, hasBody: false, hasButton: false, itemsKind: 'review', itemsLabel: 'ADMIN_CUSTOMER_REVIEWS' },
    { type: 'faq', label: 'ADMIN_BLOCK_FAQ', icon: 'quiz', hasImage: false, hasHeadline: false, hasBody: false, hasButton: false, itemsKind: 'faq', itemsLabel: 'ADMIN_QUESTIONS' },
    { type: 'cta', label: 'ADMIN_BLOCK_CTA', icon: 'campaign', hasImage: false, hasHeadline: true, hasBody: true, hasButton: true, itemsKind: null, itemsLabel: '' },
    { type: 'orderform', label: 'ADMIN_BLOCK_ORDER_FORM', icon: 'shopping_cart_checkout', hasImage: false, hasHeadline: true, hasBody: true, hasButton: true, itemsKind: null, itemsLabel: '' },
  ];

  pageId: number | null = null;
  title = signal('');
  slug = signal('');
  productId = signal<number | null>(null);
  isPublished = signal(false);
  template = signal('minimal');
  accentColor = signal('#2d5a43');
  fontFamily = signal('system');
  videoUrl = signal('');
  whatsAppNumber = signal('');
  whatsAppMessage = signal('');
  sections = signal<EditorSection[]>([]);
  products = signal<ProductDto[]>([]);

  loading = signal(true);
  saving = signal(false);
  private slugTouched = false;
  private slugifyTimer: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private landingService: LandingService,
    private productService: ProductService,
    private toastr: ToastrService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  ngOnInit(): void {
    const param = this.route.snapshot.paramMap.get('id');
    this.loadProducts();

    if (param === 'new') {
      this.loading.set(false);
      return;
    }

    const id = Number(param);
    if (!Number.isInteger(id) || id < 1) {
      this.router.navigate(['/admin/landing']);
      return;
    }

    this.landingService.getById(id).subscribe({
      next: (response) => {
        const page = response?.data;
        if (!page) {
          this.toastr.error('Landing page not found.', 'Landing Pages');
          this.router.navigate(['/admin/landing']);
          return;
        }
        this.pageId = page.id;
        this.title.set(page.title);
        this.slug.set(page.slug);
        this.slugTouched = true;
        this.productId.set(page.productId ?? null);
        this.isPublished.set(page.isPublished);
        this.template.set(page.template || 'minimal');
        this.accentColor.set(page.accentColor || '#4f46e5');
        this.fontFamily.set(page.fontFamily || 'system');
        this.videoUrl.set(page.videoUrl || '');
        this.whatsAppNumber.set(page.whatsAppNumber || '');
        this.whatsAppMessage.set(page.whatsAppMessage || '');
        this.sections.set(
          page.sections.map((s) => ({
            sectionType: s.sectionType,
            title: s.title,
            isVisible: s.isVisible,
            imageUrl: s.imageUrl,
            content: LandingService.parseContent(s),
          }))
        );
        const selectedProduct = this.products().find((product) => product.id === page.productId);
        if (selectedProduct && this.sections().every((section) => !section.isVisible)) {
          this.applyProductTemplate(selectedProduct);
        }
        this.loading.set(false);
      },
      error: () => {
        this.toastr.error('Could not load the landing page.', 'Landing Pages');
        this.router.navigate(['/admin/landing']);
      },
    });
  }

  private loadProducts(): void {
    this.productService.getAll({ pageNumber: 1, pageSize: 100, maxPageSize: 100 }).subscribe({
      next: (result) => {
        // ProductController returns the paginated items in `data`; `products`
        // is kept as a fallback for older API responses.
        const payload = result as typeof result & { data?: ProductDto[] };
        const products = payload?.data ?? result?.products ?? [];
        this.products.set(products);
        const selected = products.find((product) => product.id === this.productId());
        if (selected && this.sections().every((section) => !section.isVisible)) {
          this.applyProductTemplate(selected);
        }
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.products.set([]);
      },
    });
  }

  onProductChange(value: string): void {
    const id = value ? this.toNumber(value) : null;
    this.productId.set(id);
    if (!id) return;

    const product = this.products().find((item) => item.id === id);
    if (product) {
      this.applyProductTemplate(product);
    }
  }

  private applyProductTemplate(product: ProductDto): void {
    const firstImage = product.photos?.[0] ?? '';
    const secondImage = product.photos?.[1] ?? firstImage;
    const description = product.description?.trim() ?? '';
    this.videoUrl.set('https://interactive-examples.mdn.mozilla.net/media/cc0-videos/flower.mp4');
    this.sections.set([
      {
        sectionType: 'hero',
        title: product.name,
        isVisible: true,
        imageUrl: firstImage,
        content: {
          headline: product.name,
          body: 'اختيار عملي بجودة تستحقها. اكتشف التفاصيل واطلبه الآن بسهولة.',
          buttonText: 'احجزه الآن',
          buttonLink: '#order',
        },
      },
      {
        sectionType: 'trustbar',
        title: '',
        isVisible: true,
        imageUrl: '',
        content: {
          items: [
            { icon: 'verified', title: 'منتج موثوق' },
            { icon: 'local_shipping', title: 'شحن سريع' },
            { icon: 'support_agent', title: 'دعم متواصل' },
          ],
        },
      },
      {
        sectionType: 'features',
        title: 'لماذا ستختاره؟',
        isVisible: true,
        imageUrl: '',
        content: {
          items: [
            { icon: 'workspace_premium', title: 'جودة مميزة', text: 'تفاصيل مصممة لتقدم لك تجربة أفضل كل يوم.' },
            { icon: 'favorite', title: 'اختيار ذكي', text: 'قيمة حقيقية ومواصفات تناسب احتياجك.' },
            { icon: 'shopping_bag', title: 'اطلب بسهولة', text: 'خطوات بسيطة وسريعة من الصفحة مباشرة.' },
          ],
        },
      },
      {
        sectionType: 'showcase',
        title: 'المنتج',
        isVisible: true,
        imageUrl: secondImage,
        content: {
          headline: product.name,
          body: description || 'كل ما تحتاجه في منتج واحد، بتصميم أنيق وتجربة استخدام سهلة.',
          buttonText: 'احجز المنتج',
          buttonLink: '#order',
        },
      },
      {
          sectionType: 'reviews',
          title: 'آراء العملاء',
          isVisible: true,
          imageUrl: '',
          content: {
            items: [
              { name: 'سارة محمد', rating: 5, text: 'الخامة ممتازة والشكل أجمل من الصور. وصلت بسرعة والتغليف كان محترم.' },
              { name: 'محمود علي', rating: 5, text: 'طلبت بسهولة ووصل المنتج في معاده. تجربة شراء مريحة جدًا.' },
              { name: 'نور أحمد', rating: 4, text: 'منتج عملي والجودة واضحة من أول استخدام.' },
            ],
          },
        },
      {
          sectionType: 'faq',
          title: 'أسئلة قبل الطلب',
          isVisible: true,
          imageUrl: '',
          content: {
            items: [
              { question: 'هل يمكن معاينة المنتج قبل الدفع؟', answer: 'نعم، يمكنك معاينة المنتج عند الاستلام قبل الدفع.' },
              { question: 'متى يصل الطلب؟', answer: 'عادة يصل خلال 2 إلى 4 أيام عمل حسب المحافظة.' },
              { question: 'هل يوجد استبدال أو استرجاع؟', answer: 'نعم، نوفر استبدالًا أو استرجاعًا خلال 14 يومًا حسب سياسة المتجر.' },
            ],
          },
        },
      {
        sectionType: 'cta',
        title: '',
        isVisible: true,
        imageUrl: '',
        content: {
          headline: 'جاهز تخلي المنتج ده جزء من يومك؟',
          body: 'اطلب الآن واستمتع بتجربة شراء سهلة وآمنة.',
          buttonText: 'ابدأ طلبك الآن',
          buttonLink: '#order',
        },
      },
      {
        sectionType: 'orderform',
        title: 'احجز منتجك بسهولة',
        isVisible: true,
        imageUrl: '',
        content: {
          headline: product.name,
          body: 'أكمل بيانات الطلب وسنتواصل معك لتأكيده.',
          buttonText: 'تأكيد الطلب',
          buttonLink: '',
        },
      },
    ]);
  }

  onTitleInput(value: string): void {
    this.title.set(value);
    if (!this.slugTouched) this.slug.set(this.slugify(value));
  }

  onSlugInput(value: string): void {
    this.slugTouched = true;
    // debounce the slugify cleanup so typing feels natural
    if (isPlatformBrowser(this.platformId)) {
      clearTimeout(this.slugifyTimer);
      this.slugifyTimer = setTimeout(() => this.slug.set(this.slugify(value)), 300);
    }
  }

  toNumber(value: string | null): number {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : 0;
  }

  toRating(value: string | null): number {
    const parsed = this.toNumber(value);
    return Math.min(5, Math.max(1, parsed || 5));
  }

  private slugify(value: string): string {
    return value
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9\u0600-\u06FF\s-]/g, '')
      .replace(/[\s_]+/g, '-')
      .replace(/-+/g, '-')
      .replace(/^-|-$/g, '')
      .slice(0, 60);
  }

  defFor(section: EditorSection): SectionDef {
    return (
      this.sectionDefs.find((d) => d.type === section.sectionType) ?? {
        type: 'features',
        label: String(section.sectionType),
        icon: 'web',
        hasImage: false,
        hasHeadline: false,
        hasBody: false,
        hasButton: false,
        itemsKind: null,
        itemsLabel: '',
      }
    );
  }

  addSection(type: string): void {
    if (!type || type === 'separator') return;
    const def = this.sectionDefs.find((d) => d.type === type);
    if (!def) return;

    const section: EditorSection = {
      sectionType: type,
      title: def.label,
      isVisible: true,
      imageUrl: '',
      content: {
        headline: def.hasHeadline ? def.label : '',
        body: '',
        buttonText: def.hasButton ? 'Shop Now' : '',
        buttonLink: '',
        items: def.itemsKind === 'icon' ? [{ icon: 'check_circle', title: 'Benefit', text: '' }] : undefined,
      },
    };

    this.sections.update((list) => [...list, section]);
  }

  removeSection(index: number): void {
    this.sections.update((list) => list.filter((_, i) => i !== index));
  }

  moveSection(index: number, direction: -1 | 1): void {
    const target = index + direction;
    this.sections.update((list) => {
      if (target < 0 || target >= list.length) return list;
      const copy = [...list];
      [copy[index], copy[target]] = [copy[target], copy[index]];
      return copy;
    });
  }

  toggleVisible(section: EditorSection): void {
    section.isVisible = !section.isVisible;
  }

  addItem(section: EditorSection): void {
    const def = this.defFor(section);
    const item: LandingSectionItem =
      def.itemsKind === 'faq'
        ? { question: 'New question?', answer: '' }
        : def.itemsKind === 'review'
          ? { name: 'Customer', rating: 5, text: '' }
          : { icon: 'check_circle', title: 'Point', text: '' };

    section.content.items = [...(section.content.items ?? []), item];
  }

  removeItem(section: EditorSection, index: number): void {
    section.content.items = (section.content.items ?? []).filter((_, i) => i !== index);
  }

  save(): void {
    if (!this.title().trim()) {
      this.toastr.warning('Please enter a page title.', 'Landing Pages');
      return;
    }
    if (!this.slug().trim()) {
      this.toastr.warning('Please enter a link slug.', 'Landing Pages');
      return;
    }

    this.saving.set(true);
    const dto: SaveLandingPageDto = {
      id: this.pageId ?? 0,
      title: this.title().trim(),
      slug: this.slug().trim(),
      productId: this.productId(),
      template: this.template(),
      accentColor: this.accentColor(),
      fontFamily: this.fontFamily(),
      videoUrl: this.videoUrl().trim(),
      whatsAppNumber: this.whatsAppNumber().trim(),
      whatsAppMessage: this.whatsAppMessage().trim(),
      isPublished: this.isPublished(),
      sections: this.sections().map((section, index) => ({
        sectionType: section.sectionType,
        title: section.title ?? '',
        sortOrder: index,
        isVisible: section.isVisible,
        imageUrl: section.imageUrl ?? '',
        contentJson: JSON.stringify(section.content ?? {}),
      })),
    };

    this.landingService.save(dto).subscribe({
      next: (response) => {
        this.saving.set(false);
        this.toastr.success(
          this.isPublished() ? 'Landing page published!' : 'Landing page saved as draft.',
          'Landing Pages'
        );
        const saved = response?.data;
        if (saved) {
          this.router.navigate(['/admin/landing']);
        }
      },
      error: (error) => {
        this.saving.set(false);
        const message = error?.error?.message ?? 'Could not save the landing page.';
        this.toastr.error(message, 'Landing Pages');
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/admin/landing']);
  }
}
