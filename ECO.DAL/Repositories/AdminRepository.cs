using ECO.DAL.Data;
using ECO.DAL.Entities.OrderEntities;
using ECO.DAL.Interfaces;
using ECO.DAL.Sharing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECO.DAL.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext dbContext;

        public AdminRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private static readonly Status[] _paidStatuses = { Status.PaymentRecevied, Status.Shipped, Status.Delivered };

        public async Task<decimal> GetPaidRevenueAsync(CancellationToken cancellationToken = default)
        {
            return await dbContext.Orders.AsNoTracking()
                .Where(order => _paidStatuses.Contains(order.Status))
                .SumAsync(order => order.SubTotal + order.ShippingPrice - order.Discount, cancellationToken);
        }

        public async Task<(decimal ThisMonth, decimal LastMonth)> GetMonthlyPaidRevenueAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = monthStart.AddMonths(-1);

            var monthly = await dbContext.Orders.AsNoTracking()
                .Where(order => _paidStatuses.Contains(order.Status) && order.OrderDate >= lastMonthStart)
                .GroupBy(order => order.OrderDate >= monthStart)
                .Select(group => new { IsThisMonth = group.Key, Revenue = group.Sum(order => order.SubTotal + order.ShippingPrice - order.Discount) })
                .ToListAsync(cancellationToken);

            var thisMonth = monthly.FirstOrDefault(entry => entry.IsThisMonth)?.Revenue ?? 0m;
            var lastMonth = monthly.FirstOrDefault(entry => !entry.IsThisMonth)?.Revenue ?? 0m;
            return (thisMonth, lastMonth);
        }

        public async Task<(int ThisMonth, int LastMonth)> GetMonthlyOrderCountsAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = monthStart.AddMonths(-1);

            var monthly = await dbContext.Orders.AsNoTracking()
                .Where(order => order.OrderDate >= lastMonthStart)
                .GroupBy(order => order.OrderDate >= monthStart)
                .Select(group => new { IsThisMonth = group.Key, Count = group.Count() })
                .ToListAsync(cancellationToken);

            var thisMonth = monthly.FirstOrDefault(entry => entry.IsThisMonth)?.Count ?? 0;
            var lastMonth = monthly.FirstOrDefault(entry => !entry.IsThisMonth)?.Count ?? 0;
            return (thisMonth, lastMonth);
        }

        public async Task<int> GetOrdersCountAsync(CancellationToken cancellationToken = default)
        {
            return await dbContext.Orders.AsNoTracking().CountAsync(cancellationToken);
        }

        public async Task<int> GetUsersCountAsync(CancellationToken cancellationToken = default)
        {
            return await dbContext.Users.AsNoTracking().CountAsync(cancellationToken);
        }

        public async Task<int> GetProductsCountAsync(CancellationToken cancellationToken = default)
        {
            return await dbContext.Products.AsNoTracking().CountAsync(cancellationToken);
        }

        public async Task<List<SalesPointRow>> GetSalesLastDaysAsync(int days, CancellationToken cancellationToken = default)
        {
            var fromDate = DateTime.Today.AddDays(-(days - 1));

            var sales = await dbContext.Orders.AsNoTracking()
                .Where(order => _paidStatuses.Contains(order.Status) && order.OrderDate >= fromDate)
                .GroupBy(order => order.OrderDate.Date)
                .Select(group => new SalesPointRow
                {
                    Date = group.Key,
                    Revenue = group.Sum(order => order.SubTotal + order.ShippingPrice - order.Discount),
                    Orders = group.Count()
                })
                .ToListAsync(cancellationToken);

            // Fill missing days so the chart is always continuous.
            var byDate = sales.ToDictionary(point => point.Date, point => point);
            var result = new List<SalesPointRow>();
            for (var day = fromDate; day <= DateTime.Today; day = day.AddDays(1))
            {
                if (byDate.TryGetValue(day, out var point))
                {
                    result.Add(point);
                }
                else
                {
                    result.Add(new SalesPointRow { Date = day, Revenue = 0m, Orders = 0 });
                }
            }
            return result;
        }

        public async Task<List<RecentOrderRow>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default)
        {
            return await dbContext.Orders.AsNoTracking()
                .OrderByDescending(order => order.OrderDate)
                .Take(count)
                .Select(order => new RecentOrderRow
                {
                    Id = order.Id,
                    BuyerEmail = order.BuyerEmail,
                    Total = order.SubTotal + order.ShippingPrice - order.Discount,
                    Status = order.Status.ToString(),
                    OrderDate = order.OrderDate,
                    ItemsCount = order.OrderItems.Count()
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<TopProductRow>> GetTopProductsAsync(int count, CancellationToken cancellationToken = default)
        {
            return await dbContext.OrderItems.AsNoTracking()
                .GroupBy(item => item.ProductItemId)
                .Select(group => new TopProductRow
                {
                    Name = group.First().ProductName,
                    QuantitySold = group.Sum(item => item.Quantity),
                    Revenue = group.Sum(item => item.Price * item.Quantity)
                })
                .OrderByDescending(row => row.QuantitySold)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CustomerRow>> GetCustomersAsync(string? search, string? role, int skip, int take, CancellationToken cancellationToken = default)
        {
            var usersQuery = dbContext.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                usersQuery = usersQuery.Where(user =>
                    user.Email!.Contains(term) ||
                    (user.DisplayName != null && user.DisplayName.Contains(term)) ||
                    (user.UserName != null && user.UserName.Contains(term)));
            }

            if (role?.ToLower() == "admin")
                usersQuery = usersQuery.Where(user => dbContext.UserRoles.Any(userRole => userRole.UserId == user.Id && dbContext.Roles.Any(identityRole => identityRole.Id == userRole.RoleId && identityRole.Name == "Admin")));
            else if (role?.ToLower() == "customer")
                usersQuery = usersQuery.Where(user => !dbContext.UserRoles.Any(userRole => userRole.UserId == user.Id && dbContext.Roles.Any(identityRole => identityRole.Id == userRole.RoleId && identityRole.Name == "Admin")));

            var customers = await usersQuery
                .GroupJoin(
                    dbContext.Orders,
                    user => user.Email,
                    order => order.BuyerEmail,
                    (user, orders) => new CustomerRow
                    {
                        Id = user.Id,
                        DisplayName = user.DisplayName ?? user.UserName ?? user.Email ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        OrdersCount = orders.Count(),
                        TotalSpent = orders.Sum(order => (decimal?)(order.SubTotal + order.ShippingPrice - order.Discount)) ?? 0m
                    })
                .OrderByDescending(row => row.TotalSpent)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return customers;
        }

        public async Task<int> GetCustomersCountAsync(string? search, string? role, CancellationToken cancellationToken = default)
        {
            var usersQuery = dbContext.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                usersQuery = usersQuery.Where(user =>
                    user.Email!.Contains(term) ||
                    (user.DisplayName != null && user.DisplayName.Contains(term)) ||
                    (user.UserName != null && user.UserName.Contains(term)));
            }

            if (role?.ToLower() == "admin")
                usersQuery = usersQuery.Where(user => dbContext.UserRoles.Any(userRole => userRole.UserId == user.Id && dbContext.Roles.Any(identityRole => identityRole.Id == userRole.RoleId && identityRole.Name == "Admin")));
            else if (role?.ToLower() == "customer")
                usersQuery = usersQuery.Where(user => !dbContext.UserRoles.Any(userRole => userRole.UserId == user.Id && dbContext.Roles.Any(identityRole => identityRole.Id == userRole.RoleId && identityRole.Name == "Admin")));

            return await usersQuery.CountAsync(cancellationToken);
        }

        public async Task<List<LowStockRow>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default)
        {
            return await dbContext.Products
                .Where(product => product.TrackStock && product.StockQuantity != null && product.StockQuantity <= threshold)
                .OrderBy(product => product.StockQuantity)
                .Take(6)
                .Select(product => new LowStockRow
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    StockQuantity = product.StockQuantity
                })
                .ToListAsync(cancellationToken);
        }
    }
}
