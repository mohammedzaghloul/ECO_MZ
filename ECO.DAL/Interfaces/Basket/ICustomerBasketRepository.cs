using ECO.DAL.Entites.Basket;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Interfaces.Basket
{
    public interface ICustomerBasketRepository
    {
        Task<CustomerBasket> GetBasketAsync(string id);
        Task<CustomerBasket> UpdateAsync(CustomerBasket  customerBasket);
        Task<bool> DeleteAsync(string id);
    }
}
