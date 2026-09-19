import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { ApiEnvelope } from '../../shared/Models/api/common.models';

/** Matches ECO.BLL.DTO.AdminDtos.DashboardSummaryDto. */
export interface DashboardSummary {
  totalRevenue: number;
  totalOrders: number;
  totalCustomers: number;
  totalProducts: number;
  revenueGrowth: number;
  ordersGrowth: number;
  averageOrderValue: number;
  sales: SalesPoint[];
  recentOrders: RecentOrder[];
  topProducts: TopProduct[];
  lowStock: LowStockItem[];
}

export interface SalesPoint {
  date: string;
  revenue: number;
  orders: number;
}

export interface RecentOrder {
  id: number;
  buyerEmail: string;
  total: number;
  status: string;
  orderDate: string;
  itemsCount: number;
}

export interface TopProduct {
  name: string;
  quantitySold: number;
  revenue: number;
}

export interface LowStockItem {
  productId: number;
  name: string;
  stockQuantity: number | null;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly baseUrl = `${environment.basurl}Dashboard`;

  constructor(private http: HttpClient) {}

  getSummary(forceRefresh = false): Observable<ApiEnvelope<DashboardSummary>> {
    return this.http.get<ApiEnvelope<DashboardSummary>>(`${this.baseUrl}/Summary`, {
      params: forceRefresh ? { forceRefresh: true } : {},
      withCredentials: true,
    });
  }
}
