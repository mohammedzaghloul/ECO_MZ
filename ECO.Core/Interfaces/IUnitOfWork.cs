using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;    
        public Task<int> CompleteAsync();
    }
}
