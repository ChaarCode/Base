using CharCode.Base.Models;
using System.Threading.Tasks;

namespace CharCode.Base.Abstraction
{
    public interface IUserRepository<T> : IBaseRepository<T, string> where T : User, new()
    {
        Task ChangePasswordAsync(T user, string password);
        Task ChangePasswordAsync(T user, string currentPassword, string newPassword);
        Task<T> GetByUserNameAsync(string userName);
        Task<T> InsertAsync(T entity, string password);
        Task<string> LoginAsync(string userName, string password);
        Task LogoutAsync();
    }
}