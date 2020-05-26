using CharCode.Base.Abstraction;
using CharCode.Base.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CharCode.Base.Repositories
{
    public abstract class UserRepository<T, TDbContext> : BaseRepository<T, string, TDbContext>, IUserRepository<T>
        where T : User, new()
        where TDbContext : DbContext
    {
        protected readonly UserManager<User> userManager;
        protected readonly SignInManager<User> signInManager;
        protected readonly IConfiguration configuration;

        public UserRepository(
            TDbContext bringoDbContext,
            IConfiguration configuration,
            UserManager<User> userManager,
            SignInManager<User> signInManager) : base(bringoDbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }

        public virtual async Task<T> InsertAsync(T entity, string password)
        {
            var userName = entity.UserName.ToUpper();

            if (await IsExist(userName))
                throw new ArgumentException("نام کاربری انتخاب شده موجود می‌باشد!");

            var result = await userManager.CreateAsync(entity, password);

            return entity;
        }

        private async Task<bool> IsExist(string userName)
        {
            return await Context.Set<User>().AnyAsync(u => u.NormalizedUserName.Equals(userName));
        }

        public override async Task UpdateAsync(string id, T entity)
        {
            if (entity is null || !id.Equals(entity.Id))
                throw new ArgumentException();

            var result = await userManager.UpdateAsync(entity);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                var errorsMessage = string.Join("، ", errors);
                throw new Exception(errorsMessage);
            }
        }
    }
}
