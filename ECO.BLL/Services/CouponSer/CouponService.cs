using ECO.BLL.DTO.Basket;
using ECO.DAL.Entities;
using ECO.DAL.Interfaces;
using ECO.DAL.Specifications;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECO.BLL.Services.CouponSer
{

    public class CouponService : ICouponService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CouponService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CouponResultDto> ApplyAsync(ApplyCouponDto dto, CancellationToken cancellationToken = default)
        {
            var code = dto.Code.Trim().ToUpperInvariant();

            var basket = await _unitOfWork.CustomerBasketRepository.GetBasketAsync(dto.BasketId, cancellationToken);
            if (basket is null || !basket.BasketItems.Any())
                return new CouponResultDto { Success = false, Message = "Your basket is empty." };

            var discount = await _unitOfWork.Repository<Discount>().FirstOrDefaultAsync(
                new DiscountSpecification(code), cancellationToken);

         {   if (discount is null || !discount.IsActive)
                return new CouponResultDto { Success = false, Message = $"Coupon '{code}' is not valid." };

            if (discount.ExpiryDate.HasValue && discount.ExpiryDate.Value < DateTime.UtcNow)
                return new CouponResultDto { Success = false, Message = $"Coupon '{code}' has expired." };

                if (discount.MaxUses.HasValue && discount.UsedCount >= discount.MaxUses.Value)
                    return new CouponResultDto { Success = false, Message = $"Coupon '{code}' has reached its usage limit." };
            }

            var subTotal = basket.BasketItems.Sum(item => item.Price * item.Quantity);
            var discountAmount = discount.IsPercentage
                ? Math.Round(subTotal * discount.Value / 100m, 2)
                : Math.Min(discount.Value, subTotal);

            basket.CouponCode = code;
            await _unitOfWork.CustomerBasketRepository.UpdateAsync(basket, cancellationToken);

            return new CouponResultDto
            {
                Success = true,
                Message = $"Coupon '{code}' applied.",
                Code = code,
                IsPercentage = discount.IsPercentage,
                Value = discount.Value,
                DiscountAmount = discountAmount
            };
        }

        public async Task<CouponResultDto> RemoveAsync(string basketId, CancellationToken cancellationToken = default)
        {
            var basket = await _unitOfWork.CustomerBasketRepository.GetBasketAsync(basketId, cancellationToken);
            if (basket is null)
                return new CouponResultDto { Success = false, Message = "Basket not found." };

            basket.CouponCode = null;
            await _unitOfWork.CustomerBasketRepository.UpdateAsync(basket, cancellationToken);
            return new CouponResultDto { Success = true, Message = "Coupon removed." };
        }
    }
}
