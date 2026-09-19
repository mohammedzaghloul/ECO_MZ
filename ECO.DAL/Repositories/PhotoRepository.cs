using ECO.DAL.Entities.Product;
using ECO.DAL.Interfaces;
using ECO.DAL.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Repositories
{
    public class PhotoRepository : GenericRepository<Photo>, IPhotoRepository
    {
        public PhotoRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
