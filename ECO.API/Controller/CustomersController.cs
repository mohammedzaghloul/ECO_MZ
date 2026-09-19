using ECO.Api.Helper;
using ECO.BLL.DTO.AdminDtos;
using ECO.BLL.Services.AdminSer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECO.Api.Controller
{
    [Authorize(Roles = "Admin")]
    public class CustomersController : BaseController
    {
        private readonly ICustomerAdminService _customerService;

        public CustomersController(ICustomerAdminService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var customers = await _customerService.GetCustomersAsync(search, role, pageNumber, pageSize, cancellationToken);
            var totalCount = await _customerService.GetCustomersCountAsync(search, role, cancellationToken);

            return Ok(new Pagination<CustomerListItemDto>(pageNumber, totalCount, pageSize, customers));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            var customer = await _customerService.GetCustomerAsync(id, cancellationToken);
            if (customer is null)
                return NotFound(new ResponseApi(404, "Customer not found"));

            return Ok(new GenericResponseApi<CustomerDetailDto>(200, "Customer retrieved successfully", customer));
        }

        [HttpPut("{id}/admin-role")]
        public async Task<IActionResult> SetAdminRole(string id, [FromBody] UpdateAdminRoleDto dto, CancellationToken cancellationToken)
        {
            if (dto is null)
                return BadRequest(new ResponseApi(400, "Role data is null"));

            var isUpdated = await _customerService.SetAdminRoleAsync(id, dto.AddToRole, cancellationToken);
            if (!isUpdated)
                return NotFound(new ResponseApi(404, "Customer not found"));

            return Ok(new ResponseApi(200, dto.AddToRole ? "Admin role granted." : "Admin role removed."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var deleted = await _customerService.DeleteCustomerAsync(id, cancellationToken);
            if (!deleted)
                return BadRequest(new ResponseApi(400, "Customer was not found or cannot be deleted."));

            return Ok(new ResponseApi(200, "Customer deleted successfully."));
        }
    }
}
