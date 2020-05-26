using CharCode.Base.Models;
using System.Threading.Tasks;

namespace CharCode.Base.Abstraction
{
    public interface IAuthenticationRepository
    {
        Task ChangePasswordAsync(User user, string password);
        Task ChangePasswordAsync(User user, string currentPassword, string newPassword);
        Task ChangePasswordAsync(string userId, string password);
        Task ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<User> GetByUserNameAsync(string userName);
        Task<string> LoginAsync(string userName, string password);
        Task LogoutAsync();
    }
}