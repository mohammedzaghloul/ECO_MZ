using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Sharing
{
    /// <summary>Lightweight query result rows for the admin dashboard.</summary>
    public class SalesPointRow
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class RecentOrderRow
    {
        public int Id { get; set; }
        public string BuyerEmail { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public int ItemsCount { get; set; }
    }

    public class TopProductRow
    {
        public string Name { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class LowStockRow
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? StockQuantity { get; set; }
    }

    public class CustomerRow
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
