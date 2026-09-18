using ECO.BLL.Services.Basket;
using ECO.DAL.Entites.Basket;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECO.Api.Controller
{
    public class BasketController : BaseController
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }
        [HttpGet]
        public async Task<ActionResult<CustomerBasket>> Get(string id)
        {
            return await _basketService.GetBasketAsync(id);
        }
        [HttpDelete]
        public async Task<ActionResult<bool>> Delete(string id)
        {
            return await _basketService.DeleteAsync(id);
        }
        [HttpPost]
        public async Task<ActionResult<CustomerBasket>> Update(CustomerBasket customerBasket)
        {
            return await _basketService.UpdateAsync(customerBasket);
        }
    }
}
