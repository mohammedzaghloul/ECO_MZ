/** Matches ECO.DAL.Entites.Basket.CustomerBasket returned by the Basket API. */
export interface BasketItemDto {
  id: number;
  name: string;
  quantity: number;
  image: string;
  price: number;
  category: string;
}

export interface CustomerBasketDto {
  id: string;
  basketItems: BasketItemDto[];
  paymentIntentId?: string;
  clientSecret?: string;
}

/** Matches ECO.BLL.DTO.Basket.UpdateBasketDto. */
export interface UpdateBasketItemDto {
  productId: number;
  quantity: number;
}

export interface UpdateBasketDto {
  id: string;
  items: UpdateBasketItemDto[];
}
