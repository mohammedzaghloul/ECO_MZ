using ECO.DAL.Sharing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ECO.DAL.Interfaces
{
    public interface IAdminRepository
    {
        Task<decimal> GetPaidRevenueAsync(CancellationToken cancellationToken = default);
        Task<(decimal ThisMonth, decimal LastMonth)> GetMonthlyPaidRevenueAsync(CancellationToken cancellationToken = default);
        Task<(int ThisMonth, int LastMonth)> GetMonthlyOrderCountsAsync(CancellationToken cancellationToken = default);
        Task<int> GetOrdersCountAsync(CancellationToken cancellationToken = default);
        Task<int> GetUsersCountAsync(CancellationToken cancellationToken = default);
        Task<int> GetProductsCountAsync(CancellationToken cancellationToken = default);
        Task<List<SalesPointRow>> GetSalesLastDaysAsync(int days, CancellationToken cancellationToken = default);
        Task<List<RecentOrderRow>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default);
        Task<List<TopProductRow>> GetTopProductsAsync(int count, CancellationToken cancellationToken = default);
        Task<List<LowStockRow>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default);
        Task<List<CustomerRow>> GetCustomersAsync(string? search, string? role, int skip, int take, CancellationToken cancellationToken = default);
        Task<int> GetCustomersCountAsync(string? search, string? role, CancellationToken cancellationToken = default);
    }
}
