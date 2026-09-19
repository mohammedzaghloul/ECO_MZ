export interface IBasketItem {

    id: number;
    name: string;
    quantity: number,
    image: string,
    price: number,
    category: string

}
export interface IBasket {
    id: string;
    basketItems: IBasketItem[]
}
