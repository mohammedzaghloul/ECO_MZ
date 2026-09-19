using ECO.DAL.Interfaces;
using ECO.DAL.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ECO.DAL.Interfaces.Basket;
using ECO.DAL.Entities.Basket;
using ECO.DAL.Repositories.Basket;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;

namespace ECO.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<Type, object> _repositories = new();
        private readonly AppDbContext dbContext;
        
        public IProductRepository ProductRepository { get; }

        public ICustomerBasketRepository CustomerBasketRepository {  get; }

        public IAddressRepository AddressRepository { get; }

        public IAdminRepository AdminRepository { get; }

        private readonly IConnectionMultiplexer _redis;
        public UnitOfWork(AppDbContext dbContext,IConnectionMultiplexer redis)
        {
            this.dbContext = dbContext;
            _redis = redis;
            ProductRepository = new ProductRepository(dbContext);
            CustomerBasketRepository = new CustomerBasketRepository(_redis);
            AddressRepository = new AddressRepository(dbContext);
            AdminRepository = new AdminRepository(dbContext);

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
        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
         return await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<IDbContextTransaction> BeginTransactionAsync(
           CancellationToken cancellationToken = default)
        {
           return dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }
    }
}
