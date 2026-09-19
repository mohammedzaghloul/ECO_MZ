

export interface IProduct {
    id: number
    name: string
    newPrice: number
    oldPrice: number
    categoryId: number
    categoryName?: string
    trackStock?: boolean
    stockQuantity?: number | null
    photos: string[]
    description: string
    averageRating?: number
    reviewCount?: number
    specifications?: ProductSpecification[]
}

export interface ProductSpecification {
    label: string
    value: string
    sortOrder: number
}
