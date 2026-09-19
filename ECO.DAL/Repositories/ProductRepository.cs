using ECO.DAL.Data;
using ECO.DAL.Entities.Product;
using ECO.DAL.Interfaces;
using ECO.DAL.Sharing;
using ECO.DAL.Specifications;
using Microsoft.EntityFrameworkCore;

namespace ECO.DAL.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly AppDbContext dbContext;

        public ProductRepository(AppDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync(
            ProductParams? productParams,
            CancellationToken cancellationToken = default)
        {
            if (string.Equals(productParams?.Sort, "bestSelling", StringComparison.OrdinalIgnoreCase))
            {
                var query = dbContext.Products
                    .Include(product => product.Category)
                    .Include(product => product.Photos)
                    .Include(product => product.Reviews)
                    .Include(product => product.Specifications)
                    .AsQueryable();

                if (productParams.CategoryId.HasValue)
                {
                    query = query.Where(product => product.CategoryId == productParams.CategoryId.Value);
                }

                var searchTerm = productParams.Search ?? productParams.Serach;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var normalizedSearch = searchTerm.Trim().ToLower();
                    query = query.Where(product =>
                        product.Name.ToLower().Contains(normalizedSearch) ||
                        product.Description.ToLower().Contains(normalizedSearch));
                }

                var rankedProducts = query
                    .GroupJoin(
                        dbContext.OrderItems,
                        product => product.Id.ToString(),
                        item => item.ProductItemId,
                        (product, items) => new
                        {
                            Product = product,
                            QuantitySold = items.Sum(item => (int?)item.Quantity) ?? 0
                        })
                    .OrderByDescending(item => item.QuantitySold)
                    .ThenBy(item => item.Product.Name)
                    .Select(item => item.Product);

                if (productParams.PageNumber.HasValue && productParams.PageSize.HasValue)
                {
                    var skip = (productParams.PageNumber.Value - 1) * productParams.PageSize.Value;
                    rankedProducts = rankedProducts
                        .Skip(skip)
                        .Take(productParams.PageSize.Value);
                }

                return await rankedProducts.ToListAsync(cancellationToken);
            }

            var specification = new ProductSpecification(productParams);
            return await ListAsync(specification, cancellationToken);
        }

        public async Task<int> CountAsync(
            ProductParams? productParams,
            CancellationToken cancellationToken = default)
        {
            var specification = new ProductSpecification(productParams);
            return await CountAsync(specification, cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetByIdsAsync(
            IReadOnlyCollection<int> ids,
            CancellationToken cancellationToken = default)
        {
            if (ids.Count == 0)
                return Array.Empty<Product>();

            return await dbContext.Products
                .Include(product => product.Category)
                .Include(product => product.Photos)
                .Where(product => ids.Contains(product.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> TryDecrementStockAsync(
            int productId,
            int quantity,
            CancellationToken cancellationToken = default)
        {
            if (productId <= 0 || quantity <= 0)
                return false;

            var affectedRows = await dbContext.Products
                .Where(product =>
                    product.Id == productId &&
                    product.TrackStock &&
                    product.StockQuantity >= quantity)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        product => product.StockQuantity,
                        product => product.StockQuantity - quantity),
                    cancellationToken);

            return affectedRows == 1;
        }
    }
}
