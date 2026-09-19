import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';

export interface EmailSettings {
  smtpHost: string | null;
  smtpPort: number | null;
  smtpUsername: string | null;
  smtpFrom: string | null;
  smtpUseSsl: boolean;
  passwordConfigured: boolean;
}

export interface SaveEmailSettings {
  smtpHost: string;
  smtpPort: number;
  smtpUsername: string;
  smtpFrom: string;
  password?: string;
  smtpUseSsl: boolean;
}

export interface EmailAppearance {
  accentColor: string;
  backgroundColor: string;
  paperColor: string;
  inkColor: string;
  headingFont: string;
  logoUrl: string | null;
  headerStyle: 'minimal' | 'accent';
  brandName: string;
  footerNote: string | null;
  eyebrowText: string;
  greetingText: string;
  introText: string;
  ctaText: string;
}

@Injectable({ providedIn: 'root' })
export class StoreSettingsService {
  readonly currency = signal('EGP');
  readonly currencyLabel = signal('جنيه مصري');

  constructor(private readonly http: HttpClient) {}

  getEmailSettings() {
    return this.http.get<EmailSettings>(`${environment.basurl}StoreSettings/email`, { withCredentials: true });
  }

  saveEmailSettings(settings: SaveEmailSettings) {
    return this.http.put<{ passwordConfigured: boolean }>(
      `${environment.basurl}StoreSettings/email`,
      settings,
      { withCredentials: true }
    );
  }

  testEmail() {
    return this.http.post<{ message: string }>(
      `${environment.basurl}StoreSettings/email/test`,
      {},
      { withCredentials: true }
    );
  }

  getEmailAppearance() {
    return this.http.get<EmailAppearance>(
      `${environment.basurl}EmailSettings`,
      { withCredentials: true }
    );
  }

  saveEmailAppearance(appearance: EmailAppearance) {
    return this.http.put<EmailAppearance>(
      `${environment.basurl}EmailSettings`,
      appearance,
      { withCredentials: true }
    );
  }

  getEmailPreviewHtml(type?: string) {
    return this.http.get(`${environment.basurl}EmailSettings/preview`, {
      params: type ? { type } : {},
      responseType: 'text',
      withCredentials: true,
    });
  }

  getLiveEmailPreviewHtml(appearance: EmailAppearance, type?: string) {
    return this.http.post(`${environment.basurl}EmailSettings/preview`, appearance, {
      params: type ? { type } : {},
      responseType: 'text',
      withCredentials: true,
    });
  }

  sendEmailTestCopy(appearance: EmailAppearance, recipient?: string, templateType?: string) {
    return this.http.post<{ message: string }>(
      `${environment.basurl}EmailSettings/test`,
      { ...appearance, recipient: recipient || null, templateType: templateType || 'order' },
      { withCredentials: true }
    );
  }

  load(): void {
    this.http.get<{ currencyCode?: string }>(`${environment.basurl}StoreSettings/currency`)
      .subscribe({
        next: response => this.currency.set(response.currencyCode || 'EGP'),
        error: () => this.currency.set('EGP')
      });
  }

  saveCurrency(currencyCode: string) {
    return this.http.put<{ currencyCode: string }>(`${environment.basurl}StoreSettings`, { currencyCode });
  }
}
