using ECO.DAL.Interfaces;
using ECO.DAL.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Repostories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<Type, object> _repositories = new();
        private readonly AppDbContext dbContext;

        public IProductRepository ProductRepository { get; }

        public UnitOfWork(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
            ProductRepository = new ProductRepository(dbContext);

        }
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

            if (_repositories.TryGetValue(type, out var repository))
            {
                return (IGenericRepository<T>)repository;
            }

            var newRepository =
                new GenericRepository<T>(dbContext);

            _repositories[type] = newRepository;

            return newRepository;
        }
        public async Task<int> CompleteAsync()
        {
         return await   dbContext.SaveChangesAsync();   
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }
    }
}
