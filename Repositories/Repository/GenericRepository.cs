using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository;
using Repositories.Paging;

namespace Repositories.Repository
{
    public class GenericRepository<T> : IGenericRepository<T>
        where T : class
    {
        protected readonly DrugUsePreventionDBContext _context; // Use DatabaseContext, not DbContext
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(DrugUsePreventionDBContext context) // Accept DatabaseContext here
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        // Queryable property for LINQ queries
        public IQueryable<T> Entities => _dbSet.AsQueryable();

        // Non-async methods
        public IEnumerable<T> GetAll() => _dbSet.ToList();

        public T? GetById(object id) => _dbSet.Find(id);

        public void Insert(T obj) => _dbSet.Add(obj);

        public void InsertRange(IList<T> objs) => _dbSet.AddRange(objs);

        public void Update(T obj) => _dbSet.Update(obj);

        public void Delete(object id)
        {
            var entity = _dbSet.Find(id);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public void Save() => _context.SaveChanges();

        // Async methods
        public async Task<IList<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<BasePaginatedList<T>> GetPagging(
            IQueryable<T> query,
            int index,
            int pageSize
        )
        {
            var totalCount = await query.CountAsync();
            var items = await query.Skip((index - 1) * pageSize).Take(pageSize).ToListAsync();
            return new BasePaginatedList<T>(items, totalCount, index, pageSize);
        }

        public async Task<T?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);

        public async Task InsertAsync(T obj) => await _dbSet.AddAsync(obj);

        public async Task UpdateAsync(T obj)
        {
            _dbSet.Update(obj);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(object id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();

        public async Task<int> CountAsync() => await _dbSet.CountAsync();
    }
}
