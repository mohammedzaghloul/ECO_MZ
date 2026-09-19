export interface RegisterDto {
  email: string;
  password: string;
  userName: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface ActiveAccountDto {
  email: string;
  token: string;
}

export interface UserDto {
  id: string;
  displayName: string;
  email: string;
  roles?: string[];
}

/** Matches ECO.BLL.DTO.AddressDto (the API also emits legacy "fristName" for old clients). */
export interface AddressDto {
  firstName: string;
  lastName: string;
  city: string;
  zipCode: string;
  street: string;
  state: string;
  country: string;
}
