using ECO.Api.Helper;
using ECO.BLL.DTO.AdminDtos;
using ECO.BLL.Services.AdminSer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECO.Api.Controller
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : BaseController
    {
        private readonly IAdminDashboardService _dashboardService;

        public DashboardController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("Summary")]
        public async Task<IActionResult> Summary(
            [FromQuery] bool forceRefresh = false,
            CancellationToken cancellationToken = default)
        {
            var summary = await _dashboardService.GetSummaryAsync(forceRefresh, cancellationToken);
            return Ok(new GenericResponseApi<DashboardSummaryDto>(200, "Dashboard summary retrieved successfully", summary));
        }
    }
}
