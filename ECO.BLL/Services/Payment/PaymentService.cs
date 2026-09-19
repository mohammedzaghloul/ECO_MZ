using ECO.DAL.Entities.Basket;
using ECO.DAL.Entities.OrderEntities;
using ECO.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IConfiguration configuration;

        public PaymentService(IUnitOfWork unitOfWork,IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
        }
        public async Task<CustomerBasket> CreateOrUpadetPaymentAsync(string BasketId,int? DeliveryMehtodId,CancellationToken cancellationToken)
        {
            var customerBasket = await unitOfWork.CustomerBasketRepository.GetBasketAsync(BasketId);
            StripeConfiguration.ApiKey = configuration["StripeSetting:Secretkey"];
            var ShippingPrice = 0m;
            if (DeliveryMehtodId.HasValue)
            {
                var delivery = await unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(DeliveryMehtodId.Value, cancellationToken);
                ShippingPrice = delivery.Price;
            }
            foreach (var item in customerBasket.BasketItems)
            {
                var Product=await unitOfWork.ProductRepository.GetByIdAsync(item.Id, cancellationToken);
                item.Price = Product.NewPrice;
            } 
            PaymentIntentService paymentIntentService=new PaymentIntentService();
            PaymentIntent paymentIntent;
            if (string.IsNullOrEmpty(customerBasket.PaymentIntentId))
            {
                var option = new PaymentIntentCreateOptions
                {
                    Amount = (long)(customerBasket.BasketItems.Sum(m => m.Quantity * m.Price) * 100 + ShippingPrice * 100),
                    Currency="USD",
                    PaymentMethodTypes= new List<string> { "card"}
                };
                paymentIntent=await paymentIntentService.CreateAsync(option);
                customerBasket.PaymentIntentId=paymentIntent.Id;
                customerBasket.ClientSecret=paymentIntent.ClientSecret;
            }
            else
            {
                var option = new PaymentIntentUpdateOptions
                {
                    Amount = (long)(customerBasket.BasketItems.Sum(m => m.Quantity * m.Price) * 100 + ShippingPrice * 100),
                };
                await paymentIntentService.UpdateAsync(customerBasket.PaymentIntentId, option);
            }
            await unitOfWork.CustomerBasketRepository.UpdateAsync(customerBasket);
            return customerBasket;
        }
    }
}
