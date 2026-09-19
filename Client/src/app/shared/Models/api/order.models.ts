export interface ShippingAddressDto {
  /** The backend DTO intentionally uses this spelling. */
  fristName: string;
  lastName: string;
  city: string;
  zipCode: string;
  street: string;
  state: string;
  country?: string;
}

/** Request and current response DTO used by the order endpoints. */
export interface OrderDto {
  deliveryMethodId: number;
  governorateId: number;
  cityId: number;
  basketId: string;
  shippingAddressDto: ShippingAddressDto;
  paymentMethod?: string;
}

export interface DeliveryMethodDto {
  id: number;
  name: string;
  price: number;
  deliveryTime: string;
  description: string;
  logoUrl: string;
}
