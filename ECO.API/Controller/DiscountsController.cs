using ECO.Api.Helper;
using ECO.BLL.DTO.AdminDtos;
using ECO.BLL.Services.AdminSer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECO.Api.Controller
{
    [Authorize(Roles = "Admin")]
    public class DiscountsController : BaseController
    {
        private readonly IAdminDiscountService _discountService;

        public DiscountsController(IAdminDiscountService discountService)
        {
            _discountService = discountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var discounts = await _discountService.GetAllAsync(cancellationToken);
            return Ok(new GenericResponseApi<IReadOnlyList<DiscountDto>>(200, "Discounts retrieved successfully", discounts));
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] SaveDiscountDto dto, CancellationToken cancellationToken)
        {
            if (dto is null)
                return BadRequest(new ResponseApi(400, "Discount data is null"));

            try
            {
                var saved = await _discountService.SaveAsync(dto, cancellationToken);
                return Ok(new GenericResponseApi<DiscountDto>(200, "Discount saved successfully", saved));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ResponseApi(409, ex.Message));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var isDeleted = await _discountService.DeleteAsync(id, cancellationToken);
            if (!isDeleted)
                return NotFound(new ResponseApi(404, "Discount not found"));

            return Ok(new ResponseApi(200, "Discount deleted successfully"));
        }
    }
}
