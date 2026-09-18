export class ProductParams {
  categoryId: number | null = null;
  sort: string = 'name';
  search: string = '';
  pageNumber: number = 1;
  pageSize: number = 10;
}
