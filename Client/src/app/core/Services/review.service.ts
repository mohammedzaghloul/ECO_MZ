import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { ApiEnvelope } from '../../shared/Models/api/common.models';

/** Matches ECO.BLL.DTO.ReviewDtos.ReviewDto. */
export interface ReviewDto {
  id: number;
  productId: number;
  userId: string;
  userName: string;
  rating: number;
  title?: string | null;
  comment: string;
  createdAt: string;
  verified: boolean;
}

export interface AddReviewPayload {
  productId: number;
  rating: number;
  title?: string;
  comment: string;
}

export interface UpdateReviewPayload {
  rating: number;
  title?: string;
  comment: string;
}

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private readonly baseUrl = `${environment.basurl}Reviews`;

  constructor(private http: HttpClient) {}

  getForProduct(productId: number): Observable<ApiEnvelope<ReviewDto[]>> {
    return this.http.get<ApiEnvelope<ReviewDto[]>>(`${this.baseUrl}/Product/${productId}`);
  }

  add(payload: AddReviewPayload): Observable<ApiEnvelope<ReviewDto>> {
    return this.http.post<ApiEnvelope<ReviewDto>>(this.baseUrl, payload, { withCredentials: true });
  }

  update(id: number, payload: UpdateReviewPayload): Observable<ApiEnvelope<never>> {
    return this.http.put<ApiEnvelope<never>>(`${this.baseUrl}/${id}`, payload, { withCredentials: true });
  }

  delete(id: number): Observable<ApiEnvelope<never>> {
    return this.http.delete<ApiEnvelope<never>>(`${this.baseUrl}/${id}`, { withCredentials: true });
  }
}
