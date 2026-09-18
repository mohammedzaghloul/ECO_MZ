using ECO.DAL.Interfaces.Basket;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;    
        IProductRepository ProductRepository { get; }
        public Task<int> CompleteAsync();
        public ICustomerBasketRepository CustomerBasketRepository { get; }
    }
}
