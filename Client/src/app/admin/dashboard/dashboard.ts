import { Component, OnInit, signal, computed } from '@angular/core';
import { DashboardService, DashboardSummary } from '../../core/Services/dashboard.service';
import { LanguageService } from '../../core/Services/language.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class AdminDashboard implements OnInit {
  summary = signal<DashboardSummary | null>(null);
  loading = signal(true);
  loadError = signal(false);

  sales = computed(() => this.summary()?.sales ?? []);
  maxRevenue = computed(() => Math.max(...this.sales().map((s) => s.revenue), 1));
  weekRevenue = computed(() => this.sales().reduce((sum, p) => sum + (p.revenue ?? 0), 0));
  recentOrders = computed(() => this.summary()?.recentOrders?.slice(0, 5) ?? []);
  topProducts = computed(() => this.summary()?.topProducts ?? []);
  maxTopRevenue = computed(() => Math.max(...this.topProducts().map((p) => p.revenue ?? 0), 1));
  lowStock = computed(() => this.summary()?.lowStock ?? []);

  constructor(
    private dashboardService: DashboardService,
    private languageService: LanguageService
  ) {}

  get today(): string {
    return new Date().toLocaleDateString(
      this.languageService.currentLang() === 'ar' ? 'ar-EG' : 'en-US',
      { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }
    );
  }

  growthText(growth: number | undefined): string {
    if (growth === undefined || growth === null) return '';
    return `${this.growthLabel(growth)} ${this.languageService.t('ADMIN_THIS_MONTH')}`;
  }

  ngOnInit(): void {
    this.refresh(false);
  }

  refresh(forceRefresh = true): void {
    this.loading.set(true);
    this.loadError.set(false);
    this.dashboardService.getSummary(forceRefresh).subscribe({
      next: (response) => {
        this.summary.set(response?.data ?? null);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.loadError.set(true);
      },
    });
  }

  money(value: number | null | undefined): string {
    return `$${(value ?? 0).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  /** Compact label for chart bar tops — full amounts live in the tooltip. */
  moneyShort(value: number | null | undefined): string {
    const v = value ?? 0;
    if (v >= 1000) return `$${(v / 1000).toFixed(v >= 10000 ? 0 : 1)}k`;
    return `$${v.toFixed(v % 1 === 0 ? 0 : 2)}`;
  }

  salesLabel(date: string): string {
    const language = this.languageService.currentLang() === 'ar' ? 'ar-EG' : 'en-US';
    const parsed = new Date(`${date}, ${new Date().getFullYear()}`);
    if (Number.isNaN(parsed.getTime())) return date;
    return parsed.toLocaleDateString(language, { day: 'numeric', month: 'short' });
  }

  barHeight(revenue: number): number {
    return revenue <= 0 ? 0 : Math.max(4, Math.round((revenue / this.maxRevenue()) * 100));
  }

  topWidth(revenue: number): number {
    return revenue <= 0 ? 0 : Math.max(5, Math.round((revenue / this.maxTopRevenue()) * 100));
  }

  growthLabel(growth: number | undefined): string {
    if (growth === undefined || growth === null) return '';
    return `${growth >= 0 ? '+' : ''}${growth}%`;
  }

}
