using ECO.DAL.Interfaces;
using ECO.DAL.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECO.DAL.Repostories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext dbContext;

        public GenericRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddAsync(T entity)
        {
            await dbContext.Set<T>().AddAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await dbContext.Set<T>().FindAsync(id);

            if (entity is null)
                return;

            dbContext.Set<T>().Remove(entity);

        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await dbContext.Set<T>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(
            params Expression<Func<T, object>>[] includes)
        {
            var query = dbContext.Set<T>().AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await dbContext.Set<T>().FindAsync(id);
        }

        public async Task<T?> GetByIdAsync( int id,params Expression<Func<T, object>>[] includes)
        {
            if (id <= 0)
                return null;

            if (includes.Length == 0)
                return await dbContext.Set<T>().FindAsync(id);

            var query = dbContext.Set<T>().AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.AsNoTracking().FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public async Task UpdateAsync(T entity)
        {
            dbContext.Set<T>().Update(entity);

        }
    }
}