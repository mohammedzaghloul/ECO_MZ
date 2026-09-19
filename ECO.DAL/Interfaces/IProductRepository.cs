using ECO.DAL.Sharing;
using ECO.DAL.Entities.Product;
using ECO.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        new Task<IReadOnlyList<Product>> GetAllAsync(
            ProductParams? productParams,
            CancellationToken cancellationToken = default);
        Task<int> CountAsync(
            ProductParams? productParams,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Product>> GetByIdsAsync(
            IReadOnlyCollection<int> ids,
            CancellationToken cancellationToken = default);
        Task<bool> TryDecrementStockAsync(
            int productId,
            int quantity,
            CancellationToken cancellationToken = default);
    }
}
