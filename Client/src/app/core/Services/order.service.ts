import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { ApiEnvelope } from '../../shared/Models/api/common.models';
import { DeliveryMethodDto, OrderDto } from '../../shared/Models/api/order.models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly baseUrl = `${environment.basurl}Order`;

  constructor(private http: HttpClient) {}

  createOrder(dto: OrderDto): Observable<ApiEnvelope<OrderDto>> {
    return this.http.post<ApiEnvelope<OrderDto>>(`${this.baseUrl}/CreateOrder`, dto, { withCredentials: true });
  }

  getAllOrdersForUser(): Observable<ApiEnvelope<OrderDto[]>> {
    return this.http.get<ApiEnvelope<OrderDto[]>>(`${this.baseUrl}/GetAllOrderForUser`, { withCredentials: true });
  }

  getOrderByIdForUser(id: number): Observable<ApiEnvelope<OrderDto>> {
    return this.http.get<ApiEnvelope<OrderDto>>(`${this.baseUrl}/GetOrderByIdForUser/${id}`, { withCredentials: true });
  }

  getDeliveryMethods(): Observable<ApiEnvelope<DeliveryMethodDto[]>> {
    return this.http.get<ApiEnvelope<DeliveryMethodDto[]>>(`${this.baseUrl}/GetDeliveryMethod`);
  }
}
