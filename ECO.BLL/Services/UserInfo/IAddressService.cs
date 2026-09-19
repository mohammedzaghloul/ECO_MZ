using ECO.BLL.DTO.AddressDtos;
using ECO.BLL.DTO.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.UserInfo
{
    public interface IAddressService
    {
        public Task<AddressDto?> GetUserAddressAsync();
        public Task<AddressDto> UpdateAsync(ShippingAddressDto shippingAddressDto);
        public Task<AddressDto> UpdateByEmailAsync(string email, ShippingAddressDto shippingAddressDto);
    }
}
