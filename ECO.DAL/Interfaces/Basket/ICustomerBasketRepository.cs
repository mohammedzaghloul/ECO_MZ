using ECO.DAL.Entities.Basket;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Interfaces.Basket
{
    public interface ICustomerBasketRepository
    {
        Task<CustomerBasket> GetBasketAsync(string id, CancellationToken cancellationToken = default);
        Task<CustomerBasket> UpdateAsync(CustomerBasket customerBasket, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
