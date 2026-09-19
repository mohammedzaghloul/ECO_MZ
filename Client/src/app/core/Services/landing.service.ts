import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { ApiEnvelope } from '../../shared/Models/api/common.models';

export type LandingSectionType =
  | 'hero'
  | 'trustbar'
  | 'features'
  | 'showcase'
  | 'reviews'
  | 'faq'
  | 'cta'
  | 'orderform';

/** Matches ECO.BLL.DTO.LandingDtos.LandingSectionDto. */
export interface LandingSection {
  id?: number;
  sectionType: LandingSectionType | string;
  title: string;
  sortOrder: number;
  isVisible: boolean;
  imageUrl: string;
  contentJson: string;
}

/** Matches ECO.BLL.DTO.LandingDtos.LandingPageDto. */
export interface LandingPageDto {
  id: number;
  title: string;
  slug: string;
  productId?: number | null;
  template: string;
  accentColor: string;
  fontFamily: string;
  videoUrl: string;
  whatsAppNumber: string;
  whatsAppMessage: string;
  isPublished: boolean;
  viewCount: number;
  createdAt: string;
  updatedAt: string;
  sections: LandingSection[];
}

/** Matches ECO.BLL.DTO.LandingDtos.LandingPageSummaryDto. */
export interface LandingPageSummary {
  id: number;
  title: string;
  slug: string;
  productId?: number | null;
  productName?: string | null;
  isPublished: boolean;
  viewCount: number;
  ordersCount: number;
  conversionRate: number;
  sectionsCount: number;
  updatedAt: string;
}

/** Matches ECO.BLL.DTO.LandingDtos.SaveLandingPageDto. */
export interface SaveLandingPageDto {
  id: number;
  title: string;
  slug: string;
  productId?: number | null;
  template: string;
  accentColor: string;
  fontFamily: string;
  videoUrl: string;
  whatsAppNumber: string;
  whatsAppMessage: string;
  isPublished: boolean;
  sections: LandingSection[];
}

export interface GuestOrderRequest {
  landingPageId: number;
  productId: number;
  selectedSize?: string;
  quantity: number;
  customerName: string;
  phone: string;
  governorateId: number;
  cityId: number;
  address: string;
  notes?: string;
}

export interface GuestOrderResponse {
  orderId: number;
  trackingCode: string;
}

export interface TrackLandingEventDto {
  landingPageId: number;
  sessionId: string;
  eventType: 'visit' | 'form_started' | 'order_submitted';
  source?: string;
}

export interface LandingAnalyticsSummary {
  landingPageId: number;
  visits: number;
  uniqueVisitors: number;
  formStarts: number;
  orders: number;
  conversionRate: number;
  sources: { source: string; visits: number }[];
  timeline: { date: string; visits: number; orders: number }[];
}

/** Flexible per-section content, stored as JSON in `contentJson`. */
export interface LandingSectionContent {
  headline?: string;
  body?: string;
  buttonText?: string;
  buttonLink?: string;
  items?: LandingSectionItem[];
}

export interface LandingSectionItem {
  icon?: string;
  title?: string;
  text?: string;
  image?: string;
  name?: string;
  rating?: number;
  question?: string;
  answer?: string;
}

@Injectable({ providedIn: 'root' })
export class LandingService {
  private readonly baseUrl = `${environment.basurl}LandingPages`;

  constructor(private http: HttpClient) {}

  trackEvent(dto: TrackLandingEventDto): Observable<ApiEnvelope<null>> {
    return this.http.post<ApiEnvelope<null>>(`${environment.basurl}LandingAnalytics/Track`, dto);
  }

  getAnalyticsSummary(id: number, period?: string, from?: string, to?: string): Observable<ApiEnvelope<LandingAnalyticsSummary>> {
    let params = new HttpParams();
    if (period) params = params.set('period', period);
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return this.http.get<ApiEnvelope<LandingAnalyticsSummary>>(`${environment.basurl}LandingAnalytics/${id}/Summary`, { 
      params,
      withCredentials: true 
    });
  }

  getAll(): Observable<ApiEnvelope<LandingPageSummary[]>> {
    return this.http.get<ApiEnvelope<LandingPageSummary[]>>(`${this.baseUrl}/GetAll`);
  }

  getById(id: number): Observable<ApiEnvelope<LandingPageDto>> {
    return this.http.get<ApiEnvelope<LandingPageDto>>(`${this.baseUrl}/${id}`);
  }

  getBySlug(slug: string): Observable<ApiEnvelope<LandingPageDto>> {
    return this.http.get<ApiEnvelope<LandingPageDto>>(`${this.baseUrl}/slug/${slug}`);
  }

  save(dto: SaveLandingPageDto): Observable<ApiEnvelope<LandingPageDto>> {
    return this.http.post<ApiEnvelope<LandingPageDto>>(`${this.baseUrl}/Save`, dto);
  }

  createGuestOrder(dto: GuestOrderRequest): Observable<ApiEnvelope<GuestOrderResponse>> {
    return this.http.post<ApiEnvelope<GuestOrderResponse>>(`${environment.basurl}GuestOrder/Create`, dto);
  }

  remove(id: number): Observable<ApiEnvelope<never>> {
    return this.http.delete<ApiEnvelope<never>>(`${this.baseUrl}/${id}`);
  }

  static parseContent(section: LandingSection): LandingSectionContent {
    try {
      return JSON.parse(section.contentJson || '{}') as LandingSectionContent;
    } catch {
      return {};
    }
  }
}
