import { IProduct } from "./product"

export interface Ipagination {
  pageNumber: number
  totalCount: number
  pageSize: number
  products: IProduct[]
}

