using ECO.BLL.DTO.AdminDtos;
using ECO.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace ECO.BLL.Services.AdminSer
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public AdminDashboardService(IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(
            bool forceRefresh = false,
            CancellationToken cancellationToken = default)
        {
            const string cacheKey = "admin-dashboard-summary";
            if (!forceRefresh &&
                _cache.TryGetValue(cacheKey, out DashboardSummaryDto? cachedSummary) &&
                cachedSummary is not null)
                return cachedSummary;

            var adminRepository = _unitOfWork.AdminRepository;

            var totalRevenue = await adminRepository.GetPaidRevenueAsync(cancellationToken);
            var totalOrders = await adminRepository.GetOrdersCountAsync(cancellationToken);

            var (thisMonthRevenue, lastMonthRevenue) = await adminRepository.GetMonthlyPaidRevenueAsync(cancellationToken);
            var (thisMonthOrders, lastMonthOrders) = await adminRepository.GetMonthlyOrderCountsAsync(cancellationToken);

            var sales = await adminRepository.GetSalesLastDaysAsync(7, cancellationToken);
            var recentOrders = await adminRepository.GetRecentOrdersAsync(6, cancellationToken);
            var topProducts = await adminRepository.GetTopProductsAsync(5, cancellationToken);
            var lowStock = await adminRepository.GetLowStockAsync(5, cancellationToken);

            var summary = new DashboardSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalCustomers = await adminRepository.GetUsersCountAsync(cancellationToken),
                TotalProducts = await adminRepository.GetProductsCountAsync(cancellationToken),
                RevenueGrowth = PercentageChange(lastMonthRevenue, thisMonthRevenue),
                OrdersGrowth = PercentageChange(lastMonthOrders, thisMonthOrders),
                AverageOrderValue = totalOrders > 0 ? Math.Round(totalRevenue / totalOrders, 2) : 0m,
                Sales = sales.Select(point => new SalesPointDto
                {
                    Date = point.Date.ToString("MMM dd"),
                    Revenue = point.Revenue,
                    Orders = point.Orders
                }).ToList(),
                RecentOrders = recentOrders.Select(order => new RecentOrderDto
                {
                    Id = order.Id,
                    BuyerEmail = order.BuyerEmail,
                    Total = order.Total,
                    Status = order.Status,
                    OrderDate = order.OrderDate,
                    ItemsCount = order.ItemsCount
                }).ToList(),
                TopProducts = topProducts.Select(product => new TopProductDto
                {
                    Name = product.Name,
                    QuantitySold = product.QuantitySold,
                    Revenue = product.Revenue
                }).ToList(),
                LowStock = lowStock.Select(product => new LowStockDto
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    StockQuantity = product.StockQuantity
                }).ToList()
            };

            _cache.Set(cacheKey, summary, TimeSpan.FromSeconds(30));
            return summary;
        }

        private static double PercentageChange(decimal previous, decimal current)
        {
            if (previous <= 0m)
                return current > 0m ? 100.0 : 0.0;

            return Math.Round((double)((current - previous) / previous) * 100.0, 1);
        }
    }
}
