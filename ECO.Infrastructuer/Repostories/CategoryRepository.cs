using ECO.Core.Entites.Product;
using ECO.Core.Interfaces;
using ECO.Infrastructuer.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.Infrastructuer.Repostories
{
    public class CategoryRepository : GenriceRepository<Category> ,ICategoryRepository
    {
        public CategoryRepository(AppDbContext dbContext) : base(dbContext)
        {
            
        }
    }
}
