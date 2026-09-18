using ECO.DAL.Entites.Product;
using ECO.DAL.Interfaces;
using ECO.DAL.Data;
using ECO.DAL.Sharing;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ECO.DAL.Repostories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly AppDbContext dbContext;

        public ProductRepository(AppDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<IReadOnlyList<Product>> GetAllAsync(ProductParams? productParams)
        {
            var query = BuildFilteredQuery(productParams);

            if (productParams?.PageNumber.HasValue == true && productParams?.PageSize.HasValue == true)
            {
                var skip = (productParams.PageNumber.Value - 1) * productParams.PageSize.Value;
                query = query.Skip(skip).Take(productParams.PageSize.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<int> CountAsync(ProductParams? productParams)
        {
            return await BuildFilteredQuery(productParams).CountAsync();
        }

        private IQueryable<Product> BuildFilteredQuery(ProductParams? productParams)
        {
            var query = dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .AsNoTracking();

            if (productParams?.CategoryId.HasValue == true)
            {
                query = query.Where(p => p.CategoryId == productParams.CategoryId.Value);
            }

            var searchTerm = productParams?.Search ?? productParams?.Serach;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedSearch = searchTerm.Trim();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(normalizedSearch.ToLower()) ||
                    p.Description.ToLower().Contains(normalizedSearch.ToLower()));
            }

            if (!string.IsNullOrEmpty(productParams?.Sort))
            {
                query = productParams.Sort switch
                {
                    "name" => query.OrderBy(p => p.Name),
                    "priceAsc" or "Asce" => query.OrderBy(p => p.NewPrice),
                    "priceDesc" or "Desc" => query.OrderByDescending(p => p.NewPrice),
                    _ => query.OrderBy(p => p.Name),
                };
            }
            else
            {
                query = query.OrderBy(p => p.Name);
            }

            return query;
        }

    }
}
