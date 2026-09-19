using ECO.DAL.Entities.Basket;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Payment
{
    public interface IPaymentService
    {
        Task<CustomerBasket> CreateOrUpadetPaymentAsync(string BasketId, int? DeliveryMehtodId, CancellationToken cancellationToken);
    }
}
