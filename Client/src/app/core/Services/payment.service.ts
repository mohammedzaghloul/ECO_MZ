import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { CustomerBasketDto } from '../../shared/Models/api/basket.models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly baseUrl = `${environment.basurl}Payment`;

  private http = inject(HttpClient);

  createOrUpdatePaymentIntent(basketId: string, deliveryMethodId: number): Observable<CustomerBasketDto> {
    const params = { basketId, deliverymethodid: deliveryMethodId };
    return this.http.post<CustomerBasketDto>(`${this.baseUrl}/Create`, null, { params, withCredentials: true });
  }
}
