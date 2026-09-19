using ECO.DAL.Data;
using ECO.DAL.Interfaces;
using ECO.DAL.Specifications;
using Microsoft.EntityFrameworkCore;

namespace ECO.DAL.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext dbContext;

        public GenericRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await dbContext.Set<T>().AddAsync(entity, cancellationToken);
        }

        public async Task<int> CountAsync(ISpecification<T>? specification = null, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Set<T>().AsNoTracking().AsQueryable();

            if (specification is not null)
            {
                // A count only needs filtering; includes and ordering add unnecessary joins.
                if (specification.Criteria is not null)
                {
                    query = query.Where(specification.Criteria);
                }
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);

            if (entity is null)
                return;

            dbContext.Set<T>().Remove(entity);
        }

        public async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Set<T>().AsQueryable();
            query = SpecificationEvaluator<T>.GetQuery(query, specification);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                return null;

            return await dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T>? specification = null, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Set<T>().AsQueryable();

            if (specification is not null)
            {
                query = SpecificationEvaluator<T>.GetQuery(query, specification);
            }

            return await query.AsSplitQuery().AsNoTracking().ToListAsync(cancellationToken);
        }

        public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            dbContext.Set<T>().Update(entity);
            return Task.CompletedTask;
        }
    }
}
