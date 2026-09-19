import { Component, OnInit, signal } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AdminLocationGovernorate, AdminService } from '../../core/Services/admin.service';

@Component({
  selector: 'app-admin-locations',
  standalone: false,
  templateUrl: './locations.html',
  styleUrl: './locations.scss'
})
export class AdminLocations implements OnInit {
  locations = signal<AdminLocationGovernorate[]>([]);
  loading = signal(true);
  saving = signal(false);
  governorate = { value: '', en: '', ar: '' };
  cityDrafts: Record<number, { value: string; en: string; ar: string; shippingPrice: number; deliveryDays: number; shippingAvailable: boolean }> = {};
  editingCityIds: Record<number, number | null> = {};
  expandedGovernorates = signal<Set<number>>(new Set());

  constructor(private admin: AdminService, private toastr: ToastrService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.admin.getLocations().subscribe({
      next: response => { this.locations.set(response.data); this.loading.set(false); },
      error: () => { this.loading.set(false); this.toastr.error('Could not load locations.', 'Locations'); }
    });
  }

  editingCity(governorateId: number): boolean {
    return this.editingCityIds[governorateId] != null;
  }

  addGovernorate(): void {
    if (!this.validNames(this.governorate)) return;
    this.saving.set(true);
    this.admin.addGovernorate(this.governorate).subscribe({
      next: response => { this.locations.update(items => [...items, response.data]); this.governorate = { value: '', en: '', ar: '' }; this.done('Governorate added.'); },
      error: error => this.fail(error)
    });
  }

  addCity(governorateId: number): void {
    const city = this.cityDrafts[governorateId];
    if (!city || !this.valid(city)) return;
    this.saving.set(true);
    this.admin.addCity(governorateId, city).subscribe({
      next: response => {
        this.locations.update(items => items.map(item => item.id === governorateId ? { ...item, cities: [...item.cities, response.data] } : item));
        this.cityDrafts[governorateId] = this.emptyLocation();
        this.editingCityIds[governorateId] = null;
        this.done('City added.');
      },
      error: error => this.fail(error)
    });
  }

  cityDraft(governorateId: number): { value: string; en: string; ar: string; shippingPrice: number; deliveryDays: number; shippingAvailable: boolean } {
    return this.cityDrafts[governorateId] ??= this.emptyLocation();
  }

  updateCityDraft(governorateId: number, field: 'value' | 'en' | 'ar' | 'shippingPrice' | 'deliveryDays' | 'shippingAvailable', value: string | number | boolean): void {
    const draft = this.cityDraft(governorateId);
    if (field === 'value' || field === 'en' || field === 'ar') {
      draft[field] = String(value);
    } else if (field === 'shippingPrice' || field === 'deliveryDays') {
      draft[field] = Number(value);
    } else {
      draft[field] = Boolean(value);
    }
  }

  editCity(governorateId: number, city: AdminLocationGovernorate['cities'][number]): void {
    this.cityDrafts[governorateId] = {
      value: city.value,
      en: city.en,
      ar: city.ar,
      shippingPrice: city.shippingPrice,
      deliveryDays: city.deliveryDays,
      shippingAvailable: city.shippingAvailable
    };
    this.editingCityIds[governorateId] = city.id;
  }

  saveCity(governorateId: number, cityId: number): void {
    const city = this.cityDrafts[governorateId];
    if (!city || !this.valid(city)) return;
    this.saving.set(true);
    this.admin.updateCity(cityId, city).subscribe({
      next: response => {
        this.locations.update(items => items.map(item => item.id === governorateId
          ? { ...item, cities: item.cities.map(existing => existing.id === cityId ? response.data : existing) }
          : item));
        this.cityDrafts[governorateId] = this.emptyLocation();
        this.editingCityIds[governorateId] = null;
        this.done('City updated.');
      },
      error: error => this.fail(error)
    });
  }

  deleteGovernorate(id: number): void {
    this.admin.deleteGovernorate(id).subscribe({ next: () => { this.locations.update(items => items.filter(item => item.id !== id)); this.toastr.success('Governorate deleted.', 'Locations'); }, error: error => this.fail(error) });
  }

  deleteCity(governorateId: number, cityId: number): void {
    this.admin.deleteCity(cityId).subscribe({ next: () => this.locations.update(items => items.map(item => item.id === governorateId ? { ...item, cities: item.cities.filter(city => city.id !== cityId) } : item)), error: error => this.fail(error) });
  }

  private valid(value: { value: string; en: string; ar: string; shippingPrice: number; deliveryDays: number }): boolean {
    if (value.value.trim() && value.en.trim() && value.ar.trim() && value.shippingPrice >= 0 && value.deliveryDays >= 0) return true;
    this.toastr.warning('Names, shipping price, and delivery days are required.', 'Locations');
    return false;
  }

  private validNames(value: { value: string; en: string; ar: string }): boolean {
    if (value.value.trim() && value.en.trim() && value.ar.trim()) return true;
    this.toastr.warning('Value, English name, and Arabic name are required.', 'Locations');
    return false;
  }

  private emptyLocation(): { value: string; en: string; ar: string; shippingPrice: number; deliveryDays: number; shippingAvailable: boolean } {
    return { value: '', en: '', ar: '', shippingPrice: 0, deliveryDays: 0, shippingAvailable: true };
  }

  private done(message: string): void { this.saving.set(false); this.toastr.success(message, 'Locations'); }
  private fail(error: any): void { this.saving.set(false); this.toastr.error(error?.error?.message ?? 'Could not save locations.', 'Locations'); }

  toggleGovernorate(id: number): void {
    const expanded = new Set(this.expandedGovernorates());
    if (expanded.has(id)) {
      expanded.delete(id);
    } else {
      expanded.add(id);
    }
    this.expandedGovernorates.set(expanded);
  }

  isExpanded(id: number): boolean {
    return this.expandedGovernorates().has(id);
  }
}
