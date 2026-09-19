using ECO.Api.Helper;
using ECO.BLL.Services.Basket;
using ECO.BLL.Services.CouponSer;
using ECO.BLL.DTO.Basket;
using ECO.DAL.Entities.Basket;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace ECO.Api.Controller
{
    public class BasketController : BaseController
    {
        private readonly IBasketService _basketService;
        private readonly ICouponService _couponService;

        public BasketController(IBasketService basketService, ICouponService couponService)
        {
            _basketService = basketService;
            _couponService = couponService;
        }
        [HttpGet]
        public async Task<ActionResult<CustomerBasket>> Get(string id, CancellationToken cancellationToken)
        {
            return await _basketService.GetBasketAsync(id, cancellationToken);
        }
        [HttpDelete]
        public async Task<ActionResult<bool>> Delete(string id, CancellationToken cancellationToken)
        {
            return await _basketService.DeleteAsync(id, cancellationToken);
        }
        [HttpPost]
        public async Task<ActionResult<CustomerBasket>> Update(
            UpdateBasketDto basketDto,
            CancellationToken cancellationToken)
        {
            return await _basketService.UpdateAsync(basketDto, cancellationToken);
        }

        [HttpPost("coupon")]
        public async Task<ActionResult<CouponResultDto>> ApplyCoupon([FromBody] ApplyCouponDto dto, CancellationToken cancellationToken)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.BasketId) || string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest(new ResponseApi(400, "Basket id and coupon code are required"));

            return await _couponService.ApplyAsync(dto, cancellationToken);
        }

        [HttpDelete("coupon")]
        public async Task<ActionResult<CouponResultDto>> RemoveCoupon([FromQuery] string basketId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(basketId))
                return BadRequest(new ResponseApi(400, "Basket id is required"));

            return await _couponService.RemoveAsync(basketId, cancellationToken);
        }
    }
}
