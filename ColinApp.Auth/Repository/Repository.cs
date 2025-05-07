using ColinApp.Auth.Entities.Base;
using ColinApp.Common.IRepository;
using ColinApp.Common.Page;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ColinApp.Auth.Repository
{
    /// <summary>
    /// 数据仓库实现
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(string id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<PagedResult<T>> GetPagedAsync(int pageIndex, int pageSize)
        {
            var query = _dbSet.AsQueryable();
            var total = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<T> { Items = items, TotalCount = total };
        }

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void SoftDelete(T entity)
        {
            _dbSet.Update(entity);
        }

        public async Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<T?> FindSingleByConditionAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
