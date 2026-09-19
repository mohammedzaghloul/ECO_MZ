import { Component, signal, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

export interface DemoReview {
  name: string;
  city: string;
  rating: number;
  text: string;
  verified: boolean;
  date: string;
}

export interface DemoFeature {
  icon: string;
  title: string;
  desc: string;
}

export interface DemoFaq {
  question: string;
  answer: string;
  open: boolean;
}

export interface DemoOrderForm {
  name: string;
  phone: string;
  governorate: string;
  city: string;
  address: string;
  notes: string;
  quantity: number;
}

@Component({
  selector: 'app-landing-demo-page',
  standalone: false,
  templateUrl: './landing-demo-page.html',
  styleUrl: './landing-demo-page.scss',
})
export class LandingDemoPage {
  /* ─── Product data ──────────────────────────────────────── */
  readonly product = {
    name: 'سماعات Pro X7 اللاسلكية',
    badge: 'الأكثر مبيعاً',
    headline: 'صوت لا مثيل له.\nتجربة لا تُنسى.',
    subheadline:
      'استمتع بجودة صوت استوديو احترافي في كل مكان — سماعات Pro X7 مع خاصية إلغاء الضوضاء النشطة وعمر بطارية 40 ساعة.',
    oldPrice: 799,
    newPrice: 450,
    discount: 44,
    inStock: true,
    stockCount: 23,
    sku: 'PRX7-BLK',
    rating: 4.9,
    reviewCount: 312,
    warranty: 'ضمان سنة',
    photos: [
      'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800&q=80',
      'https://images.unsplash.com/photo-1484704849700-f032a568e944?w=800&q=80',
      'https://images.unsplash.com/photo-1524678606370-a47ad25cb82a?w=800&q=80',
      'https://images.unsplash.com/photo-1583394838336-acd977736f90?w=800&q=80',
    ],
    specs: [
      { icon: 'bluetooth', label: 'Bluetooth 5.3', value: 'اتصال فوري وثابت' },
      { icon: 'battery_charging_full', label: 'بطارية 40 ساعة', value: 'وشحن سريع 10 دقائق = 3 ساعات' },
      { icon: 'noise_control_off', label: 'إلغاء ضوضاء نشط', value: 'ANC بتقنية هايبرد ثلاثية المراحل' },
      { icon: 'mic', label: 'ميكروفون 4×', value: 'مكالمات كريستال واضحة' },
      { icon: 'compress', label: 'تصميم فولدابل', value: 'خفيف ومريح 265 جرام فقط' },
      { icon: 'water_drop', label: 'مقاوم للماء', value: 'تصنيف IPX4' },
    ],
    description: `
      <p>سماعات <strong>Pro X7</strong> هي الرفيق المثالي لمحبي الموسيقى المحترفين والمتخصصين في العمل والدراسة.
      تجمع بين جودة الصوت الاستوديوي وأحدث تقنيات إلغاء الضوضاء في تصميم أنيق وخفيف.</p>
      <ul>
        <li>🎵 درايفر 40 ملم عالي الدقة بنطاق ترددي 20 هرتز – 20 كيلوهرتز</li>
        <li>📶 Bluetooth 5.3 بمدى 10 أمتار وتوصيل فوري</li>
        <li>🔋 40 ساعة استماع + وضع سلكي احتياطي</li>
        <li>🎧 وسادات أذن من جلد الذاكرة الناعم لراحة أطول</li>
        <li>✈️ وضع السفر مع ANC ومناسبة للطائرات والمواصلات</li>
      </ul>
    `,
  };

  /* ─── UI State ──────────────────────────────────────────── */
  activePhoto = signal(0);
  quantity = signal(1);
  submitted = signal(false);
  submitting = signal(false);
  trackingCode = signal('');
  activeTab = signal<'description' | 'specs'>('description');
  expandedFaq = signal<number | null>(null);

  form: DemoOrderForm = {
    name: '',
    phone: '',
    governorate: '',
    city: '',
    address: '',
    notes: '',
    quantity: 1,
  };

  /* ─── Static data ───────────────────────────────────────── */
  readonly governorates = [
    'القاهرة', 'الجيزة', 'الإسكندرية', 'المنصورة', 'طنطا',
    'الزقازيق', 'الإسماعيلية', 'بورسعيد', 'السويس', 'أسيوط',
    'سوهاج', 'الأقصر', 'أسوان', 'المنيا', 'قنا', 'بني سويف',
    'الفيوم', 'دمياط', 'كفر الشيخ', 'شبرا الخيمة', 'مرسى مطروح',
    'الغردقة', 'شرم الشيخ', 'العريش',
  ];

  readonly features: DemoFeature[] = [
    {
      icon: 'noise_control_off',
      title: 'إلغاء ضوضاء ANC',
      desc: 'تقنية هايبرد ثلاثية المراحل تقلل الضوضاء المحيطة بنسبة تصل إلى 95%، احتفظ بتركيزك في أي مكان.',
    },
    {
      icon: 'battery_charging_full',
      title: 'بطارية تدوم 40 ساعة',
      desc: '10 دقائق شحن سريع = 3 ساعات استماع. كيبل USB-C مدرج في الكرتونة.',
    },
    {
      icon: 'high_quality',
      title: 'جودة صوت Hi-Fi',
      desc: 'درايفر 40 ملم مخصص مع دعم كودك LDAC وAAC لتجربة صوت كـAmazon Music HD.',
    },
    {
      icon: 'self_improvement',
      title: 'راحة لساعات',
      desc: 'وسادات الذاكرة الناعمة وإطار الرأس القابل للتعديل تضمن لك راحة تامة أثناء الاستخدام المطول.',
    },
    {
      icon: 'water_drop',
      title: 'IPX4 مقاوم للماء',
      desc: 'الرذاذ والتعرق لن يؤثرا على أداء السماعة — مثالية للرياضة والسفر.',
    },
    {
      icon: 'compress',
      title: 'تصميم فولدابل',
      desc: 'تطوي في ثانية وتوضع في الحقيبة براحة تامة مع كيس حمل مميز.',
    },
  ];

  readonly reviews: DemoReview[] = [
    {
      name: 'أحمد محمود',
      city: 'القاهرة',
      rating: 5,
      text: 'والله جربت كتير بس دي الأحسن بالفرق. الصوت نقي جداً وخاصية إلغاء الضوضاء بجد تشتغل. وصلت في يومين بس!',
      verified: true,
      date: 'منذ 3 أيام',
    },
    {
      name: 'نورا سمير',
      city: 'الإسكندرية',
      rating: 5,
      text: 'اشتريتها لجلسات العمل من المنزل. الميكروفون واضح ولا يوجد أي صدى. الراحة تمام حتى بعد 4 ساعات. مريحة جداً على الأذن.',
      verified: true,
      date: 'منذ أسبوع',
    },
    {
      name: 'كريم حسام',
      city: 'الجيزة',
      rating: 5,
      text: 'البطارية مش بتخلص! شغّلتها يومين متواصلين ولسه شغّالة. الكرتونة كانت محترمة والتغليف أنيق. شكراً!',
      verified: true,
      date: 'منذ أسبوعين',
    },
    {
      name: 'منى طارق',
      city: 'المنصورة',
      rating: 4,
      text: 'منتج ممتاز ولمسة فاخرة. فضلت أسمع موسيقى 6 ساعات مش حسيت بثقل. السعر مناسب جداً للجودة دي.',
      verified: true,
      date: 'منذ 3 أسابيع',
    },
    {
      name: 'سامي يوسف',
      city: 'طنطا',
      rating: 5,
      text: 'اشتريتها لابني كهدية عيد وانبهر بيها! الاتصال فوري وما فيش تقطيع خالص. بشكرك جداً على الخدمة.',
      verified: true,
      date: 'منذ شهر',
    },
    {
      name: 'هدى إبراهيم',
      city: 'أسيوط',
      rating: 5,
      text: 'أشتري منكم للمرة الثانية ومش هفضل. التوصيل سريع والمنتج زي ما وصفتوه بالظبط. شكراً على الأمانة!',
      verified: true,
      date: 'منذ شهرين',
    },
  ];

  readonly faqs: DemoFaq[] = [
    {
      question: 'هل يمكن معاينة المنتج قبل الدفع؟',
      answer: 'بالتأكيد! التوصيل بالدفع عند الاستلام ويمكنك معاينة المنتج قبل دفع أي فلوس. لو مش رايق نرجعه بدون أي مشاكل.',
      open: false,
    },
    {
      question: 'كم يوم يستغرق التوصيل؟',
      answer: 'عادةً من 2 لـ 4 أيام عمل لمعظم محافظات مصر. القاهرة والجيزة والإسكندرية غالباً في يومين.',
      open: false,
    },
    {
      question: 'هل يوجد ضمان أو استرجاع؟',
      answer: 'نعم! ضمان سنة كاملة ضد عيوب الصناعة + 14 يوم استرجاع مجاني لو المنتج جاء مختلف عن الوصف.',
      open: false,
    },
    {
      question: 'هل السماعة متوافقة مع iPhone وAndroid؟',
      answer: 'نعم، متوافقة مع جميع الأجهزة التي تدعم Bluetooth 4.0 وما فوق — iPhone وAndroid وWindows ومنصات الألعاب.',
      open: false,
    },
    {
      question: 'هل يمكن التوصيل بأكثر من جهاز في نفس الوقت؟',
      answer: 'نعم، تدعم الـ Multipoint وتتصل بجهازين في نفس الوقت (مثلاً: الموبايل واللاب توب معاً).',
      open: false,
    },
  ];

  constructor(
    private toastr: ToastrService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  /* ─── Photo navigation ─────────────────────────────── */
  selectPhoto(i: number): void {
    this.activePhoto.set(i);
  }

  prevPhoto(): void {
    this.activePhoto.update((i) => (i - 1 + this.product.photos.length) % this.product.photos.length);
  }

  nextPhoto(): void {
    this.activePhoto.update((i) => (i + 1) % this.product.photos.length);
  }

  /* ─── Quantity ─────────────────────────────────────── */
  inc(): void { this.quantity.update((q) => Math.min(10, q + 1)); }
  dec(): void { this.quantity.update((q) => Math.max(1, q - 1)); }

  get total(): number { return this.product.newPrice * this.quantity(); }
  get savings(): number { return (this.product.oldPrice - this.product.newPrice) * this.quantity(); }

  /* ─── FAQ toggle ───────────────────────────────────── */
  toggleFaq(i: number): void {
    this.expandedFaq.update((prev) => (prev === i ? null : i));
  }

  /* ─── Scroll ───────────────────────────────────────── */
  scrollToOrder(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    document.getElementById('order-section')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  /* ─── Submit ───────────────────────────────────────── */
  submit(): void {
    const phone = this.form.phone.trim().replace(/[\s\-()]/g, '').replace(/^(\+20|0020|20)/, '0');
    if (!this.form.name.trim()) {
      this.toastr.warning('من فضلك اكتب اسمك الثلاثي.', 'بيانات مطلوبة');
      return;
    }
    if (!/^01[0125][0-9]{8}$/.test(phone)) {
      this.toastr.warning('رقم الموبايل غير صحيح. مثال: 01012345678', 'رقم الموبايل');
      return;
    }
    if (!this.form.governorate) {
      this.toastr.warning('اختر المحافظة.', 'بيانات مطلوبة');
      return;
    }
    if (!this.form.address.trim() || this.form.address.trim().length < 5) {
      this.toastr.warning('اكتب عنوانك بالتفصيل.', 'بيانات مطلوبة');
      return;
    }

    this.submitting.set(true);
    setTimeout(() => {
      const code = 'PRX-' + Math.random().toString(36).slice(2, 8).toUpperCase();
      this.trackingCode.set(code);
      this.submitted.set(true);
      this.submitting.set(false);
    }, 1400);
  }

  /* ─── Stars helper ─────────────────────────────────── */
  stars(rating: number): boolean[] {
    return [1, 2, 3, 4, 5].map((s) => s <= rating);
  }
}
