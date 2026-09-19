/** Matches ECO.Api.Helper.ResponseApi and GenericResponseApi<T>. */
export interface ApiEnvelope<T> {
  statusCode: number;
  message: string;
  data?: T | null;
}

/** Matches ECO.Api.Helper.Pagination<T>. This response is not enveloped. */
export interface PaginatedResult<T> {
  pageNumber: number;
  totalCount: number;
  pageSize: number;
  products: T[];
}

/** Query parameters accepted by GET /api/Product/GetAll. */
export interface ProductParams {
  sort?: string;
  categoryId?: number;
  pageNumber?: number;
  pageSize?: number;
  maxPageSize?: number;
  search?: string;
}
