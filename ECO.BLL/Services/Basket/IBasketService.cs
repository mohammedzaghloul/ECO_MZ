using ECO.DAL.Entites.Basket;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Basket
{
    public interface IBasketService
    {
        Task<CustomerBasket> GetBasketAsync(string id);
        Task<CustomerBasket> UpdateAsync(CustomerBasket customerBasket);
        Task<bool> DeleteAsync(string id);
    }
}
