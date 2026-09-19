using ECO.DAL.Interfaces.Basket;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;    
        IProductRepository ProductRepository { get; }
        IAddressRepository AddressRepository {  get; }
        IAdminRepository AdminRepository { get; }
        public Task<int> CompleteAsync(CancellationToken cancellationToken = default);
        public ICustomerBasketRepository CustomerBasketRepository { get; }
        Task<IDbContextTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken = default);
    }
}
