using ECO.DAL.Entites.Basket;
using ECO.DAL.Interfaces;
using ECO.DAL.Repostories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Basket
{
    public class BasketService : IBasketService
    {
        private readonly IUnitOfWork unitOfWork;

        public BasketService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public Task<bool> DeleteAsync(string id)
        {
           return unitOfWork.CustomerBasketRepository.DeleteAsync(id);
        }

        public Task<CustomerBasket> GetBasketAsync(string id)
        {
            return unitOfWork.CustomerBasketRepository.GetBasketAsync(id);
        }

        public Task<CustomerBasket> UpdateAsync(CustomerBasket customerBasket)
        {
          return unitOfWork.CustomerBasketRepository.UpdateAsync(customerBasket);
        }
    }
}
