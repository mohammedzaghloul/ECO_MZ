using ECO.DAL.Entities.Basket;
using ECO.BLL.DTO.Basket;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Basket
{
    public interface IBasketService
    {
        Task<CustomerBasket> GetBasketAsync(string id, CancellationToken cancellationToken = default);
        Task<CustomerBasket> UpdateAsync(UpdateBasketDto basketDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
