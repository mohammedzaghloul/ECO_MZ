import { Pipe, PipeTransform } from '@angular/core';
import { StoreSettingsService } from '../../core/Services/store-settings.service';

@Pipe({ name: 'storeCurrency', pure: false, standalone: false })
export class StoreCurrencyPipe implements PipeTransform {
  constructor(private readonly settings: StoreSettingsService) {}

  transform(value: number | null | undefined): string {
    return `${Number(value ?? 0).toFixed(2)} ${this.settings.currency()}`;
  }
}
