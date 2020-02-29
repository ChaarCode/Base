using CharCode.Base.Abstraction;
using CharCode.Base.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CharCode.Base.Repositories
{
    public abstract class UserRepository<T, TDbContext> : BaseRepository<T, string, TDbContext>, IUserRepository<T> 
        where T : User, new()
        where TDbContext : DbContext
    {
        public UserRepository(TDbContext bringoDbContext) : base(bringoDbContext)
        {
        }

        public async Task<T> GetByUserNameAsync(string userName)
        {
            return await GetObjects().SingleOrDefaultAsync(u => u.UserName.ToLower().Equals(userName.ToLower()));
        }
    }
}
