import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { ApiEnvelope, PaginatedResult } from '../../shared/Models/api/common.models';

export interface AdminOrder {
  id: number;
  buyerEmail: string;
  buyerPhone?: string;
  orderDate: string;
  status: string;
  subTotal: number;
  total: number;
  discount: number;
  shippingPrice: number;
  paymentMethod?: string;
  deliveryMethodId?: number;
  deliveryMethodName?: string;
  shippingAddress?: {
    fristName?: string;
    firstName?: string;
    lastName?: string;
    city?: string;
    zipCode?: string;
    street?: string;
    state?: string;
  };
  orderItems: { id: number; productName: string; price: number; quantity: number; mainImage?: string }[];
}

export interface AdminCustomer {
  id: string;
  displayName: string;
  email: string;
  ordersCount: number;
  totalSpent: number;
  roles: string[];
}

export interface AdminCustomerDetail extends AdminCustomer {
  phoneNumber: string;
  orders: AdminOrder[];
}

export interface AdminDiscount {
  id: number;
  code: string;
  isPercentage: boolean;
  value: number;
  isActive: boolean;
  expiryDate: string | null;
  maxUses: number | null;
  usedCount: number;
}

export interface SaveDiscount {
  id: number;
  code: string;
  isPercentage: boolean;
  value: number;
  isActive: boolean;
  expiryDate: string | null;
  maxUses: number | null;
}

export interface CouponResult {
  success: boolean;
  message: string;
  code?: string;
  isPercentage?: boolean;
  value?: number;
  discountAmount?: number;
}

export interface LocationCatalog {
  id: number;
  key: string;
  jsonContent: string;
  updatedAtUtc: string;
}

export interface AdminLocationCity {
  id: number;
  value: string;
  en: string;
  ar: string;
  shippingPrice: number;
  deliveryDays: number;
  shippingAvailable: boolean;
}
export interface AdminLocationGovernorate {
  id: number; value: string; en: string; ar: string; cities: AdminLocationCity[];
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly baseUrl = `${environment.basurl}`;

  constructor(private http: HttpClient) {}

  getLocationCatalog(key = 'egypt'): Observable<ApiEnvelope<LocationCatalog>> {
    return this.http.get<ApiEnvelope<LocationCatalog>>(`${this.baseUrl}Locations/${key}`, { withCredentials: true });
  }

  saveLocationCatalog(key: string, jsonContent: string): Observable<ApiEnvelope<LocationCatalog>> {
    return this.http.put<ApiEnvelope<LocationCatalog>>(`${this.baseUrl}Locations/${key}`, { key, jsonContent }, { withCredentials: true });
  }

  getLocations(): Observable<ApiEnvelope<AdminLocationGovernorate[]>> {
    return this.http.get<ApiEnvelope<AdminLocationGovernorate[]>>(`${this.baseUrl}Locations/egypt`, { withCredentials: true });
  }

  addGovernorate(dto: Omit<AdminLocationGovernorate, 'id' | 'cities'>): Observable<ApiEnvelope<AdminLocationGovernorate>> {
    return this.http.post<ApiEnvelope<AdminLocationGovernorate>>(`${this.baseUrl}Locations/governorates`, dto, { withCredentials: true });
  }

  updateGovernorate(id: number, dto: Omit<AdminLocationGovernorate, 'id' | 'cities'>): Observable<ApiEnvelope<AdminLocationGovernorate>> {
    return this.http.put<ApiEnvelope<AdminLocationGovernorate>>(`${this.baseUrl}Locations/governorates/${id}`, dto, { withCredentials: true });
  }

  deleteGovernorate(id: number): Observable<ApiEnvelope<never>> {
    return this.http.delete<ApiEnvelope<never>>(`${this.baseUrl}Locations/governorates/${id}`, { withCredentials: true });
  }

  addCity(governorateId: number, dto: Omit<AdminLocationCity, 'id'>): Observable<ApiEnvelope<AdminLocationCity>> {
    return this.http.post<ApiEnvelope<AdminLocationCity>>(`${this.baseUrl}Locations/governorates/${governorateId}/cities`, dto, { withCredentials: true });
  }

  deleteCity(id: number): Observable<ApiEnvelope<never>> {
    return this.http.delete<ApiEnvelope<never>>(`${this.baseUrl}Locations/cities/${id}`, { withCredentials: true });
  }

  updateCity(id: number, dto: Omit<AdminLocationCity, 'id'>): Observable<ApiEnvelope<AdminLocationCity>> {
    return this.http.put<ApiEnvelope<AdminLocationCity>>(`${this.baseUrl}Locations/cities/${id}`, dto, { withCredentials: true });
  }

  // ── Orders ──
  getOrders(params: { status?: string; search?: string; fromDate?: string; toDate?: string; pageNumber?: number; pageSize?: number }): Observable<PaginatedResult<AdminOrder>> {
    let httpParams = new HttpParams();
    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber);
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize);
    return this.http.get<PaginatedResult<AdminOrder>>(`${this.baseUrl}AdminOrders`, { params: httpParams, withCredentials: true });
  }

  getOrder(id: number): Observable<ApiEnvelope<AdminOrder>> {
    return this.http.get<ApiEnvelope<AdminOrder>>(`${this.baseUrl}AdminOrders/${id}`, { withCredentials: true });
  }

  updateOrderStatus(id: number, status: string): Observable<ApiEnvelope<never>> {
    return this.http.put<ApiEnvelope<never>>(`${this.baseUrl}AdminOrders/${id}/status`, { status }, { withCredentials: true });
  }

  // ── Customers ──
  getCustomers(params: { search?: string; role?: string; pageNumber?: number; pageSize?: number }): Observable<PaginatedResult<AdminCustomer>> {
    let httpParams = new HttpParams();
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.role) httpParams = httpParams.set('role', params.role);
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber);
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize);
    return this.http.get<PaginatedResult<AdminCustomer>>(`${this.baseUrl}Customers`, { params: httpParams, withCredentials: true });
  }

  getCustomer(id: string): Observable<ApiEnvelope<AdminCustomerDetail>> {
    return this.http.get<ApiEnvelope<AdminCustomerDetail>>(`${this.baseUrl}Customers/${id}`, { withCredentials: true });
  }

  setAdminRole(id: string, addToRole: boolean): Observable<ApiEnvelope<never>> {
    return this.http.put<ApiEnvelope<never>>(`${this.baseUrl}Customers/${id}/admin-role`, { addToRole }, { withCredentials: true });
  }

  deleteCustomer(id: string): Observable<ApiEnvelope<never>> {
    return this.http.delete<ApiEnvelope<never>>(`${this.baseUrl}Customers/${id}`, { withCredentials: true });
  }

  // ── Discounts ──
  getDiscounts(): Observable<ApiEnvelope<AdminDiscount[]>> {
    return this.http.get<ApiEnvelope<AdminDiscount[]>>(`${this.baseUrl}Discounts`, { withCredentials: true });
  }

  saveDiscount(dto: SaveDiscount): Observable<ApiEnvelope<AdminDiscount>> {
    return this.http.post<ApiEnvelope<AdminDiscount>>(`${this.baseUrl}Discounts/Save`, dto, { withCredentials: true });
  }

  deleteDiscount(id: number): Observable<ApiEnvelope<never>> {
    return this.http.delete<ApiEnvelope<never>>(`${this.baseUrl}Discounts/${id}`, { withCredentials: true });
  }
}
