using ECO.BLL.DTO.Basket;

namespace ECO.BLL.Services.CouponSer
{
    public interface ICouponService
    {
        Task<CouponResultDto> ApplyAsync(ApplyCouponDto dto, CancellationToken cancellationToken = default);
        Task<CouponResultDto> RemoveAsync(string basketId, CancellationToken cancellationToken = default);
    }
}
