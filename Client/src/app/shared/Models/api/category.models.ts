export interface AddCategoryDto {
  name: string;
  description: string;
}

export interface UpdateCategoryDto extends AddCategoryDto {
  id: number;
}

/** Matches ECO.BLL.DTO.CategoryDto. */
export interface CategoryDto {
  id: number;
  name: string;
  description: string;
}
