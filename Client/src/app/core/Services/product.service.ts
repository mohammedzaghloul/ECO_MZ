import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { ApiEnvelope, PaginatedResult, ProductParams } from '../../shared/Models/api/common.models';
import { ProductDto } from '../../shared/Models/api/product.models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly baseUrl = `${environment.basurl}Product`;

  constructor(private http: HttpClient) {}

  getAll(params: ProductParams = {}): Observable<PaginatedResult<ProductDto>> {
    let httpParams = new HttpParams();
    if (params.sort) httpParams = httpParams.set('Sort', params.sort);
    if (params.categoryId != null) httpParams = httpParams.set('CategoryId', params.categoryId);
    if (params.pageNumber != null) httpParams = httpParams.set('PageNumber', params.pageNumber);
    if (params.pageSize != null) httpParams = httpParams.set('PageSize', params.pageSize);
    if (params.maxPageSize != null) httpParams = httpParams.set('MaxPageSize', params.maxPageSize);
    if (params.search) httpParams = httpParams.set('Search', params.search);
    return this.http.get<PaginatedResult<ProductDto>>(`${this.baseUrl}/GetAll`, { params: httpParams, withCredentials: true });
  }

  getById(id: number): Observable<ApiEnvelope<ProductDto>> {
    return this.http.get<ApiEnvelope<ProductDto>>(`${this.baseUrl}/${id}`);
  }

  create(formData: FormData): Observable<ProductDto> {
    return this.http.post<ProductDto>(this.baseUrl, formData);
  }

  update(id: number, formData: FormData): Observable<ApiEnvelope<never>> {
    return this.http.put<ApiEnvelope<never>>(`${this.baseUrl}/${id}`, formData);
  }

  delete(id: number): Observable<ApiEnvelope<never>> {
    return this.http.delete<ApiEnvelope<never>>(`${this.baseUrl}/${id}`);
  }
}
