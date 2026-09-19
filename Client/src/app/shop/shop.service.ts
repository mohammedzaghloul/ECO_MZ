import { HttpClient, HttpContext } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Ipagination } from '../shared/Models/Pagnation';
import { map, Observable, Subject } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ICategory } from '../shared/Models/Category/Category.component';
import { IProduct } from '../shared/Models/product';
import { SKIP_GLOBAL_LOADING } from '../core/interceptors/loading.interceptor';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class ShopService {
  
  BaseUrl = signal(environment.basurl);
  searchTerm = signal('');
  private searchInput$ = new Subject<string>();

  constructor(private http: HttpClient) { }

  emitSearchInput(term: string) {
    this.searchTerm.set(term);
    this.searchInput$.next(term);
  }

  onSearchInput() {
    return this.searchInput$.asObservable();
  }

  getProduct(params?: any): Observable<Ipagination> {
    let httpParams = new HttpParams();
    
    if (params) {
      if (params.Search) {
        httpParams = httpParams.set('Search', params.Search);
        httpParams = httpParams.set('Serach', params.Search);
      }
      if (params.CategoryId != null) httpParams = httpParams.set('CategoryId', params.CategoryId);
      if (params.Sort) httpParams = httpParams.set('Sort', params.Sort);
      if (params.PageNumber) httpParams = httpParams.set('PageNumber', params.PageNumber);
      if (params.PageSize) httpParams = httpParams.set('PageSize', params.PageSize);
    }
    
    return this.http.get<Ipagination>(`${this.BaseUrl()}Product/GetAll`, {
      params: httpParams,
      context: new HttpContext().set(SKIP_GLOBAL_LOADING, true),
    });
  }

  getCategories() {
    return this.http
      .get<{ data: ICategory[] }>(`${this.BaseUrl()}Categories/GetAll`, {
        context: new HttpContext().set(SKIP_GLOBAL_LOADING, true),
      })
      .pipe(map((response) => response.data));
  }

  getProductById(id: number) {
    return this.http.get<any>(`${this.BaseUrl()}Product/${id}`);
  }

  createProduct(formData: FormData) {
    return this.http.post<IProduct>(`${this.BaseUrl()}Product`, formData);
  }

  updateProduct(id: number, formData: FormData) {
    return this.http.put(`${this.BaseUrl()}Product/${id}`, formData);
  }

  deleteProduct(id: number) {
    return this.http.delete(`${this.BaseUrl()}Product/${id}`);
  }

  createCategory(category: Pick<ICategory, 'name' | 'description'>) {
    return this.http.post<ICategory>(`${this.BaseUrl()}Categories`, category);
  }

  updateCategory(category: ICategory) {
    return this.http.put(`${this.BaseUrl()}Categories/${category.id}`, category);
  }

  deleteCategory(id: number) {
    return this.http.delete(`${this.BaseUrl()}Categories/${id}`);
  }
}
