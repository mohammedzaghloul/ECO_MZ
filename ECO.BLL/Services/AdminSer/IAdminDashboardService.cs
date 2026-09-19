using ECO.BLL.DTO.AdminDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.AdminSer
{
    public interface IAdminDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);
    }
}
