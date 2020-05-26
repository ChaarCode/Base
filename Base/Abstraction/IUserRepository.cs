using CharCode.Base.Models;
using System.Threading.Tasks;

namespace CharCode.Base.Abstraction
{
    public interface IUserRepository<T> : IBaseRepository<T, string> where T : User
    {
        Task<T> InsertAsync(T entity, string password);
    }
}