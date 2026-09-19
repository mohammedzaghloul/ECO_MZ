import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { AddCategoryDto, CategoryDto, UpdateCategoryDto } from '../../shared/Models/api/category.models';
import { ApiEnvelope } from '../../shared/Models/api/common.models';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly baseUrl = `${environment.basurl}Categories`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiEnvelope<CategoryDto[]>> {
    return this.http.get<ApiEnvelope<CategoryDto[]>>(`${this.baseUrl}/GetAll`);
  }

  getById(id: number): Observable<ApiEnvelope<CategoryDto>> {
    return this.http.get<ApiEnvelope<CategoryDto>>(`${this.baseUrl}/${id}`);
  }

  create(dto: AddCategoryDto): Observable<CategoryDto> {
    return this.http.post<CategoryDto>(this.baseUrl, dto);
  }

  update(id: number, dto: UpdateCategoryDto): Observable<ApiEnvelope<never>> {
    return this.http.put<ApiEnvelope<never>>(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: number): Observable<ApiEnvelope<never>> {
    return this.http.delete<ApiEnvelope<never>>(`${this.baseUrl}/${id}`);
  }
}
