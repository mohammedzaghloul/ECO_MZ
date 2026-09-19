import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { LandingAnalyticsSummary, LandingPageSummary, LandingService } from '../../core/Services/landing.service';
import { LanguageService } from '../../core/Services/language.service';

@Component({
  selector: 'app-admin-landing-list',
  standalone: false,
  templateUrl: './landing-list.html',
  styleUrl: './landing-list.scss',
})
export class AdminLandingList implements OnInit {
  pages = signal<LandingPageSummary[]>([]);
  loading = signal(true);
  expandedPages = signal<Set<number>>(new Set());
  analytics = signal<Record<number, LandingAnalyticsSummary | undefined>>({});
  analyticsLoading = signal<Set<number>>(new Set());
  selectedPeriod = signal<'all' | 'today' | '7d' | '30d'>('all');

  constructor(
    private landingService: LandingService,
    private router: Router,
    private toastr: ToastrService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.landingService.getAll().subscribe({
      next: (response) => {
        this.pages.set(response?.data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toastr.error('Could not load landing pages.', 'Landing Pages');
      },
    });
  }

  create(): void {
    this.router.navigate(['/admin/landing/edit', 'new']);
  }

  edit(id: number): void {
    this.router.navigate(['/admin/landing/edit', id]);
  }

  view(slug: string): void {
    window.open(`/landing/${slug}`, '_blank');
  }

  remove(page: LandingPageSummary): void {
    if (!confirm(`Delete landing page "${page.title}"? This cannot be undone.`)) return;

    this.landingService.remove(page.id).subscribe({
      next: () => {
        this.toastr.success('Landing page deleted.', 'Landing Pages');
        this.load();
      },
      error: () => this.toastr.error('Could not delete the landing page.', 'Landing Pages'),
    });
  }

  setPeriod(period: 'all' | 'today' | '7d' | '30d'): void {
    this.selectedPeriod.set(period);
    // Reload analytics for expanded pages
    this.expandedPages().forEach(id => {
      this.loadAnalytics(id, period);
    });
  }

  copyLink(slug: string, event: Event): void {
    event.stopPropagation();
    const url = `${window.location.origin}/landing/${slug}`;
    if (navigator.clipboard) {
      navigator.clipboard.writeText(url).then(
        () => this.toastr.success('تم نسخ رابط صفحة الهبوط', 'تم النسخ'),
        () => this.copyLinkFallback(url)
      );
    } else {
      this.copyLinkFallback(url);
    }
  }

  private copyLinkFallback(url: string): void {
    const input = document.createElement('textarea');
    input.value = url;
    input.setAttribute('readonly', '');
    input.style.position = 'fixed';
    input.style.opacity = '0';
    document.body.appendChild(input);
    input.select();
    const copied = document.execCommand('copy');
    input.remove();
    if (copied) {
      this.toastr.success('تم نسخ رابط صفحة الهبوط', 'تم النسخ');
    } else {
      this.toastr.info(url, 'انسخ الرابط يدوياً');
    }
  }

  togglePage(id: number): void {
    const expanded = new Set(this.expandedPages());
    if (expanded.has(id)) {
      expanded.delete(id);
    } else {
      expanded.add(id);
    }
    this.expandedPages.set(expanded);
    if (!this.analytics()[id]) {
      this.loadAnalytics(id);
    }
  }

  private loadAnalytics(id: number, period?: string): void {
    const p = period ?? this.selectedPeriod();
    this.analyticsLoading.update(ids => new Set(ids).add(id));
    this.landingService.getAnalyticsSummary(id, p === 'all' ? undefined : p).subscribe({
      next: response => {
        this.analytics.update(current => ({ ...current, [id]: response?.data ?? undefined }));
        this.analyticsLoading.update(ids => {
          const next = new Set(ids);
          next.delete(id);
          return next;
        });
      },
      error: () => {
        this.analyticsLoading.update(ids => {
          const next = new Set(ids);
          next.delete(id);
          return next;
        });
        this.toastr.error('Could not load landing analytics.', 'Landing Pages');
      },
    });
  }

  analyticsFor(id: number): LandingAnalyticsSummary | undefined {
    return this.analytics()[id];
  }

  isAnalyticsLoading(id: number): boolean {
    return this.analyticsLoading().has(id);
  }

  isExpanded(id: number): boolean {
    return this.expandedPages().has(id);
  }

  formatDate(value: string, includeTime = false): string {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;
    const locale = this.languageService.currentLang() === 'ar' ? 'ar-EG' : 'en-US';
    return new Intl.DateTimeFormat(locale, {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      ...(includeTime ? { hour: 'numeric', minute: '2-digit' } : {}),
    }).format(date);
  }

  barHeight(value: number, max: number = 60, factor: number = 4): number {
    return Math.min(max, Math.max(4, value * factor));
  }
}
