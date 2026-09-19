using ECO.BLL.DTO.OrderDtos;

namespace ECO.BLL.Services.Orderserv;

public interface IGuestOrderService
{
    Task<GuestOrderResponseDto> CreateAsync(
        GuestOrderRequestDto request,
        CancellationToken cancellationToken = default);
}
