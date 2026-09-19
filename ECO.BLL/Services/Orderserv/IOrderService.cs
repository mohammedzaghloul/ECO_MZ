using ECO.BLL.DTO.Order;
using ECO.BLL.DTO.OrderDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Orderserv
{
    public interface IOrderService
    {
        Task<OrderDto> CreateAsync(OrderDto orderDto, string BuyerEmail, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OrderToReturnDto>> GetAllOrderForUserAsync(
            string BuyerEmail,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OrderToReturnDto>> GetAllAsync(
            CancellationToken cancellationToken = default);
        Task<OrderToReturnDto?> GetOrderById(
            int Id,
            string Email,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DeliveryMethodDto>> GetDeliveryMethodAsync(
            CancellationToken cancellationToken = default);
    }
}
