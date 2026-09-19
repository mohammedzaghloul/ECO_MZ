import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  basurl = environment.basurl;

  constructor(private http: HttpClient) {}

  getOrdersForUser() {
    return this.http.get(this.basurl + 'Order/GetAllOrderForUser', { withCredentials: true });
  }

  getOrderById(id: number) {
    return this.http.get(this.basurl + `Order/GetOrderByIdForUser/${id}`, { withCredentials: true });
  }

  getDeliveryMethods() {
    return this.http.get(this.basurl + 'Order/GetDeliveryMethod', { withCredentials: true });
  }
}
