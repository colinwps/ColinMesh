using ColinApp.Common.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ColinApp.Common.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FindSingleByConditionAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdAsync(string id);
        Task<PagedResult<T>> GetPagedAsync(int pageIndex, int pageSize);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void SoftDelete(T entity);
        Task SaveChangesAsync();
    }
}
