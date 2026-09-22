
export interface ICategory {
  id: number;
  name: string;
  description: string;
  parentCategoryId?: number | null;
  parentCategory?: ICategory | null;
  subCategories?: ICategory[];
}
