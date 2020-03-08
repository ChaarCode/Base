using CharCode.Base.Classes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CharCode.Base.Abstraction
{
    public interface IBaseRepository<T, TKey> where T : IModel<TKey>
    {
        Task DeleteAsync(TKey id);
        Task DeleteAsync(IEnumerable<TKey> ids);
        Task<T> GetAsync(TKey id);
        Task<List<T>> GetAsync(PaginationConfig config);
        Task<long> GetCountAsync(PaginationConfig config);
        Task<T> InsertAsync(T entity);
        Task InsertAsync(IEnumerable<T> entities);
        Task UpdateAsync(TKey id, T entity);
        Task UpdateAsync(IEnumerable<T> entities);
    }
    public interface IBaseRepository<T> : IBaseRepository<T, long> where T : IModel
    {
    }
}
