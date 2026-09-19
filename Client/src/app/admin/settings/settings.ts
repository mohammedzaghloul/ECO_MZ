import { Component, inject, signal } from '@angular/core';
import { EmailSettings, StoreSettingsService } from '../../core/Services/store-settings.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-admin-settings',
  standalone: false,
  templateUrl: './settings.html',
  styleUrls: ['./settings.scss']
})
export class AdminSettings {
  private readonly storeSettings = inject(StoreSettingsService);
  private readonly toastr = inject(ToastrService);
  currency = signal('EGP');
  saving = signal(false);
  emailSaving = signal(false);
  emailTesting = signal(false);
  emailMessage = signal('');
  emailTestRecipient = signal('');
  emailTestState = signal<'idle' | 'success' | 'error'>('idle');
  email = signal<EmailSettings>({
    smtpHost: '',
    smtpPort: 465,
    smtpUsername: '',
    smtpFrom: '',
    smtpUseSsl: true,
    passwordConfigured: false,
  });
  emailPassword = '';

  canTestEmail(): boolean {
    const settings = this.email();
    return Boolean(
      settings.smtpHost?.trim() &&
      settings.smtpPort &&
      settings.smtpUsername?.trim() &&
      settings.smtpFrom?.trim() &&
      settings.passwordConfigured
    );
  }

  ngOnInit(): void {
    this.currency.set(this.storeSettings.currency());
    this.storeSettings.getEmailSettings().subscribe({
      next: settings => this.email.set(settings),
    });
  }

  save(): void {
    this.saving.set(true);
    this.storeSettings.saveCurrency(this.currency()).subscribe({
      next: response => {
        this.storeSettings.currency.set(response.currencyCode);
        this.saving.set(false);
        this.toastr.success('تم حفظ العملة بنجاح.', 'إعدادات المتجر');
      },
      error: () => {
        this.saving.set(false);
        this.toastr.error('تعذر حفظ العملة.', 'إعدادات المتجر');
      }
    });
  }

  saveEmail(): void {
    const settings = this.email();
    if (!settings.smtpHost?.trim() || !settings.smtpPort || !settings.smtpUsername?.trim() || !settings.smtpFrom?.trim()) {
      const message = 'أكمل بيانات خادم البريد والمرسل أولاً.';
      this.emailMessage.set(message);
      this.toastr.warning(message, 'إعدادات الإرسال');
      return;
    }
    this.emailSaving.set(true);
    this.emailMessage.set('');
    this.emailTestState.set('idle');
    this.storeSettings.saveEmailSettings({
      smtpHost: settings.smtpHost.trim(),
      smtpPort: Number(settings.smtpPort),
      smtpUsername: settings.smtpUsername.trim(),
      smtpFrom: settings.smtpFrom.trim(),
      password: this.emailPassword || undefined,
      smtpUseSsl: settings.smtpUseSsl,
    }).subscribe({
      next: response => {
        this.emailSaving.set(false);
        this.emailPassword = '';
        this.email.update(current => ({ ...current, passwordConfigured: response.passwordConfigured }));
        const message = 'تم حفظ إعدادات البريد بنجاح.';
        this.emailMessage.set(message);
        this.toastr.success(message, 'إعدادات الإرسال');
      },
      error: error => {
        this.emailSaving.set(false);
        const message = error?.error?.message ?? 'تعذر حفظ إعدادات البريد.';
        this.emailMessage.set(message);
        this.toastr.error(message, 'إعدادات الإرسال');
      },
    });
  }

  testEmail(): void {
    if (!this.canTestEmail()) {
      this.emailTestState.set('error');
      const message = 'أكمل بيانات SMTP واضغط «حفظ إعدادات البريد» أولاً، ثم أرسل رسالة الاختبار.';
      this.emailMessage.set(message);
      this.toastr.warning(message, 'اختبار البريد');
      return;
    }
    this.emailTesting.set(true);
    this.emailMessage.set('');
    this.emailTestRecipient.set('');
    this.emailTestState.set('idle');
    this.storeSettings.testEmail().subscribe({
      next: response => {
        this.emailTesting.set(false);
        this.emailTestState.set('success');
        this.emailTestRecipient.set(this.email().smtpFrom.trim());
        this.emailMessage.set(response.message);
        this.toastr.success(response.message, 'اختبار البريد');
      },
      error: error => {
        this.emailTesting.set(false);
        this.emailTestState.set('error');
        this.emailTestRecipient.set('');
        const message = error?.error?.message ?? 'تعذر إرسال رسالة الاختبار.';
        this.emailMessage.set(message);
        this.toastr.error(message, 'اختبار البريد');
      },
    });
  }
}
