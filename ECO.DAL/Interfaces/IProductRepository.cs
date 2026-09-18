using ECO.DAL.Sharing;
using ECO.DAL.Entites.Product;
using ECO.DAL.Repostories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IReadOnlyList<Product>> GetAllAsync(ProductParams? productParams);
        Task<int> CountAsync(ProductParams? productParams);
    }
}
