using ECO.DAL.Entites.Product;
using ECO.DAL.Interfaces;
using ECO.DAL.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Repostories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
