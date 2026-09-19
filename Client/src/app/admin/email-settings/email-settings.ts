import { Component, inject, signal } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { EmailAppearance, StoreSettingsService } from '../../core/Services/store-settings.service';
import { ToastrService } from 'ngx-toastr';

const DEFAULT_APPEARANCE: EmailAppearance = {
  accentColor: '#2D5A43',
  backgroundColor: '#F6F4EF',
  paperColor: '#FFFEFC',
  inkColor: '#26251F',
  headingFont: 'Noto Serif Arabic',
  logoUrl: '',
  headerStyle: 'minimal',
  brandName: 'ECO',
  footerNote: '',
  eyebrowText: 'تأكيد الطلب',
  greetingText: 'شكرًا لثقتك، {name}',
  introText: 'طلبك بين أيدينا الآن، وسنعلمك بكل تحديث عبر البريد أولًا بأول.',
  ctaText: 'متابعة تفاصيل الطلب',
};

export interface ColorPreset {
  id: string;
  name: string;
  desc: string;
  accentColor: string;
  backgroundColor: string;
  paperColor: string;
  inkColor: string;
}

const COLOR_PRESETS: ColorPreset[] = [
  {
    id: 'forest',
    name: 'أخضر هادئ (ECO)',
    desc: 'الهوية الطبيعية الافتراضية',
    accentColor: '#2D5A43',
    backgroundColor: '#F6F4EF',
    paperColor: '#FFFEFC',
    inkColor: '#26251F',
  },
  {
    id: 'ocean',
    name: 'أزرق كلاسيك',
    desc: 'طابع موثوق ورسمي',
    accentColor: '#1E40AF',
    backgroundColor: '#F0F4F8',
    paperColor: '#FFFFFF',
    inkColor: '#1E293B',
  },
  {
    id: 'sand',
    name: 'بيج دافئ ترابي',
    desc: 'طابع فاخر ومريح',
    accentColor: '#854D0E',
    backgroundColor: '#FAF7F2',
    paperColor: '#FFFDF9',
    inkColor: '#292524',
  },
];

const HEADING_FONTS = ['Noto Serif Arabic', 'IBM Plex Sans Arabic', 'Cairo', 'Tajawal', 'Amiri', 'Rubik'];

@Component({
  selector: 'app-admin-email-settings',
  standalone: false,
  templateUrl: './email-settings.html',
  styleUrls: ['./email-settings.scss']
})
export class AdminEmailSettings {
  private readonly storeSettings = inject(StoreSettingsService);
  private readonly toastr = inject(ToastrService);
  private readonly sanitizer = inject(DomSanitizer);

  appearance = signal<EmailAppearance>({ ...DEFAULT_APPEARANCE });
  activeTab = signal<'branding' | 'copy' | 'advanced'>('branding');
  saving = signal(false);
  sendingTest = signal(false);
  previewLoading = signal(false);
  previewHtml = signal<SafeHtml>('');
  testRecipient = '';
  headingFonts = HEADING_FONTS;
  presets = COLOR_PRESETS;
  showCustomColors = signal(false);
  logoError = signal(false);
  private refreshDebounce: any;

  applyPreset(preset: ColorPreset): void {
    this.appearance.update(a => ({
      ...a,
      accentColor: preset.accentColor,
      backgroundColor: preset.backgroundColor,
      paperColor: preset.paperColor,
      inkColor: preset.inkColor,
    }));
    this.applyLive();
  }

  isPresetActive(preset: ColorPreset): boolean {
    const a = this.appearance();
    const hex = (c: string) => (c || '').trim().toUpperCase();
    return hex(a.accentColor) === hex(preset.accentColor) &&
           hex(a.backgroundColor) === hex(preset.backgroundColor) &&
           hex(a.paperColor) === hex(preset.paperColor) &&
           hex(a.inkColor) === hex(preset.inkColor);
  }

  resetToDefaults(): void {
    this.appearance.set({ ...DEFAULT_APPEARANCE });
    this.logoError.set(false);
    this.refreshPreview();
    this.toastr.info('تمت استعادة الإعدادات الافتراضية الأصلية.', 'تصميم البريد');
  }

  debounceRefresh(): void {
    clearTimeout(this.refreshDebounce);
    this.refreshDebounce = setTimeout(() => {
      this.refreshPreview();
    }, 400);
  }

  onLogoUrlChange(value: string): void {
    this.appearance.update(a => ({ ...a, logoUrl: value }));
    this.logoError.set(false);
    this.debounceRefresh();
  }

  clearLogo(): void {
    this.appearance.update(a => ({ ...a, logoUrl: '' }));
    this.logoError.set(false);
    this.debounceRefresh();
  }

  onBrandNameChange(name: string): void {
    this.appearance.update(a => ({ ...a, brandName: name }));
    this.debounceRefresh();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const file = input.files[0];
    if (file.size > 2 * 1024 * 1024) {
      this.toastr.warning('حجم صورة الشعار يجب أن يكون أقل من 2 ميجابايت.', 'شعار المتجر');
      return;
    }
    const reader = new FileReader();
    reader.onload = () => {
      const dataUrl = reader.result as string;
      this.appearance.update(a => ({ ...a, logoUrl: dataUrl }));
      this.logoError.set(false);
      this.debounceRefresh();
    };
    reader.readAsDataURL(file);
    input.value = '';
  }

  /** Raw HTML as last rendered by the server, plus the values the server used. */
  private previewBase = '';
  private serverValues: EmailAppearance = { ...DEFAULT_APPEARANCE };

  ngOnInit(): void {
    this.storeSettings.getEmailAppearance().subscribe({
      next: look => this.appearance.set({ ...DEFAULT_APPEARANCE, ...look, footerNote: look.footerNote ?? '', logoUrl: look.logoUrl ?? '' }),
    });
    this.refreshPreview();
  }

  /** Re-renders the preview from the last server render, swapping colors/font live. */
  private applyLive(): void {
    if (!this.previewBase) return;
    let html = this.previewBase;
    const snap = this.serverValues;
    const cur = this.appearance();
    const hex = (c: string) => (c || '').trim().toUpperCase();
    const pairs: Array<[string, string]> = [
      [hex(snap.accentColor), hex(cur.accentColor)],
      [hex(snap.backgroundColor), hex(cur.backgroundColor)],
      [hex(snap.paperColor), hex(cur.paperColor)],
      [hex(snap.inkColor), hex(cur.inkColor)],
      [`'${snap.headingFont}'`, `'${(cur.headingFont || '').trim()}'`],
    ];
    for (const [from, to] of pairs) {
      if (from && to && from !== to) {
        html = html.split(from).join(to);
      }
    }
    this.previewHtml.set(this.sanitizer.bypassSecurityTrustHtml(html));
  }

  onColorChange(field: 'accentColor' | 'backgroundColor' | 'paperColor' | 'inkColor', value: string): void {
    const color = (value || '').trim().toUpperCase();
    if (!/^#[0-9A-F]{6}$/.test(color)) return;
    this.appearance.update(a => ({ ...a, [field]: color }));
    this.applyLive();
  }

  onFontChange(value: string): void {
    this.appearance.update(a => ({ ...a, headingFont: value }));
    this.applyLive();
  }

  previewType = signal<'order' | 'activation' | 'reset'>('order');
  readonly templateTypes = [
    { value: 'order', label: 'تأكيد الطلب (Order Confirmation)' },
    { value: 'activation', label: 'تفعيل الحساب (Account Activation)' },
    { value: 'reset', label: 'إعادة تعيين كلمة المرور (Password Reset)' },
  ];

  onPreviewTypeChange(type: 'order' | 'activation' | 'reset'): void {
    this.previewType.set(type);
    this.refreshPreview();
  }

  refreshPreview(): void {
    this.previewLoading.set(true);
    this.storeSettings.getLiveEmailPreviewHtml(this.normalize(this.appearance()), this.previewType()).subscribe({
      next: html => {
        // Angular's sanitizer would strip the email document structure — trust our own template output.
        this.previewBase = html;
        this.serverValues = { ...this.appearance() };
        this.previewHtml.set(this.sanitizer.bypassSecurityTrustHtml(html));
        this.previewLoading.set(false);
      },
      error: () => {
        // Fallback to GET preview if needed
        this.storeSettings.getEmailPreviewHtml(this.previewType()).subscribe({
          next: html => {
            this.previewBase = html;
            this.serverValues = { ...this.appearance() };
            this.previewHtml.set(this.sanitizer.bypassSecurityTrustHtml(html));
            this.previewLoading.set(false);
          },
          error: () => this.previewLoading.set(false)
        });
      },
    });
  }

  save(): void {
    const look = this.appearance();
    if (!look.brandName.trim()) {
      this.toastr.warning('اكتب اسم البراند أولاً.', 'تصميم البريد');
      return;
    }
    this.saving.set(true);
    this.storeSettings.saveEmailAppearance(this.normalize(look)).subscribe({
      next: saved => {
        this.appearance.set({ ...DEFAULT_APPEARANCE, ...saved, footerNote: saved.footerNote ?? '', logoUrl: saved.logoUrl ?? '' });
        this.saving.set(false);
        this.toastr.success('تم حفظ تصميم رسائل البريد.', 'تصميم البريد');
        this.refreshPreview();
      },
      error: error => {
        this.saving.set(false);
        this.toastr.error(error?.error?.message ?? 'تعذر حفظ التصميم.', 'تصميم البريد');
      },
    });
  }

  sendTest(): void {
    this.sendingTest.set(true);
    this.storeSettings.sendEmailTestCopy(this.normalize(this.appearance()), this.testRecipient || undefined, this.previewType()).subscribe({
      next: response => {
        this.sendingTest.set(false);
        this.toastr.success(response.message, 'نسخة تجريبية');
      },
      error: error => {
        this.sendingTest.set(false);
        this.toastr.error(error?.error?.message ?? 'تعذر إرسال النسخة التجريبية.', 'نسخة تجريبية');
      },
    });
  }

  private normalize(look: EmailAppearance): EmailAppearance {
    return {
      accentColor: look.accentColor.trim(),
      backgroundColor: look.backgroundColor.trim(),
      paperColor: look.paperColor.trim(),
      inkColor: look.inkColor.trim(),
      headingFont: look.headingFont.trim(),
      logoUrl: look.logoUrl?.trim() || null,
      headerStyle: look.headerStyle,
      brandName: look.brandName.trim(),
      footerNote: look.footerNote?.trim() || null,
      eyebrowText: look.eyebrowText?.trim() ?? '',
      greetingText: look.greetingText?.trim() ?? '',
      introText: look.introText?.trim() ?? '',
      ctaText: look.ctaText?.trim() ?? '',
    };
  }
}
