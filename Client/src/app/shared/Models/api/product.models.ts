/** Matches ECO.BLL.DTO.ProductDto. */
export interface ProductDto {
  id: number;
  name: string;
  description: string;
  newPrice: number;
  oldPrice: number;
  categoryId: number;
  categoryName?: string;
  trackStock: boolean;
  stockQuantity: number | null;
  lengthCm: number | null;
  widthCm: number | null;
  heightCm: number | null;
  weightKg: number | null;
  photos: string[];
  specifications: ProductSpecificationDto[];
}

export interface ProductSpecificationDto {
  label: string;
  value: string;
  sortOrder: number;
}
