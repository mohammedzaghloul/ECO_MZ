import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';

export interface CheckoutLocation {
  id: number;
  value: string;
  en: string;
  ar: string;
  cities: { id: number; value: string; en: string; ar: string; shippingPrice: number; deliveryDays: number; shippingAvailable: boolean }[];
}

export interface CityShipping {
  id: number;
  shippingPrice: number;
  deliveryDays: number;
  shippingAvailable: boolean;
}

@Injectable({ providedIn: 'root' })
export class LocationService {
  private readonly baseUrl = `${environment.basurl}`;

  constructor(private http: HttpClient) {}

  getEgypt(): Observable<CheckoutLocation[]> {
    return this.http
      .get<{ data: CheckoutLocation[] }>(`${this.baseUrl}Locations/egypt`)
      .pipe(map(response => response.data));
  }

  getShipping(cityId: number): Observable<CityShipping> {
    return this.http
      .get<{ data: CityShipping }>(`${this.baseUrl}Locations/shipping/${cityId}`)
      .pipe(map(response => response.data));
  }
}
