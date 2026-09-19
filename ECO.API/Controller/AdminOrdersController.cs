using ECO.Api.Helper;
using ECO.BLL.DTO.AdminDtos;
using ECO.BLL.DTO.OrderDtos;
using ECO.BLL.Services.AdminSer;
using ECO.DAL.Entities.OrderEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECO.Api.Controller
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : BaseController
    {
        private readonly IAdminOrderService _adminOrderService;

        public AdminOrdersController(IAdminOrderService adminOrderService)
        {
            _adminOrderService = adminOrderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? search,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var orders = await _adminOrderService.GetAllAsync(
                status,
                search,
                fromDate,
                toDate,
                pageNumber,
                pageSize,
                cancellationToken);
            var totalCount = await _adminOrderService.GetCountAsync(
                status,
                search,
                fromDate,
                toDate,
                cancellationToken);

            return Ok(new Pagination<OrderToReturnDto>(pageNumber, totalCount, pageSize, orders));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var order = await _adminOrderService.GetByIdAsync(id, cancellationToken);
            if (order is null)
                return NotFound(new ResponseApi(404, $"Order with id {id} was not found"));

            return Ok(new GenericResponseApi<OrderToReturnDto>(200, "Order retrieved successfully", order));
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto, CancellationToken cancellationToken)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Status))
                return BadRequest(new ResponseApi(400, "Status is required"));

            if (!Enum.TryParse<Status>(dto.Status, ignoreCase: true, out var parsed))
                return BadRequest(new ResponseApi(400, $"'{dto.Status}' is not a valid order status."));

            var isUpdated = await _adminOrderService.UpdateStatusAsync(id, parsed, cancellationToken);
            if (!isUpdated)
                return NotFound(new ResponseApi(404, $"Order with id {id} was not found"));

            return Ok(new ResponseApi(200, "Order status updated successfully"));
        }
    }
}
