using ECO.Core.Entites.Product;
using ECO.Core.Interfaces;
using ECO.Infrastructuer.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.Infrastructuer.Repostories
{
    public class ProductRepository : GenriceRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
