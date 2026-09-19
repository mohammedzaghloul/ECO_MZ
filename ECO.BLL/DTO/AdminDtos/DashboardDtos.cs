using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.DTO.AdminDtos
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public double RevenueGrowth { get; set; }
        public double OrdersGrowth { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<SalesPointDto> Sales { get; set; } = new();
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
        public List<LowStockDto> LowStock { get; set; } = new();
    }

    public class SalesPointDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class RecentOrderDto
    {
        public int Id { get; set; }
        public string BuyerEmail { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public int ItemsCount { get; set; }
    }

    public class TopProductDto
    {
        public string Name { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class LowStockDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? StockQuantity { get; set; }
    }
}
