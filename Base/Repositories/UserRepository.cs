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
        protected readonly UserManager<User> _userManager;
        protected readonly SignInManager<User> _signInManager;
        protected readonly IConfiguration _configuration;

        public UserRepository(
            TDbContext bringoDbContext,
            IConfiguration configuration,
            UserManager<User> userManager,
            SignInManager<User> signInManager) : base(bringoDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public virtual async Task<T> InsertAsync(T entity, string password)
        {
            var userName = entity.UserName.ToUpper();

            if (await IsExist(userName))
                throw new ArgumentException("نام کاربری انتخاب شده موجود می‌باشد!");

            var result = await _userManager.CreateAsync(entity, password);

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

            var result = await _userManager.UpdateAsync(entity);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                var errorsMessage = string.Join("، ", errors);
                throw new Exception(errorsMessage);
            }
        }

        public async Task ChangePasswordAsync(T user, string password)
        {
            var isValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isValid)
                throw new ArgumentException();

            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, password);
        }


        public async Task ChangePasswordAsync(T user, string currentPassword, string newPassword)
        {
            var isValid = await _userManager.CheckPasswordAsync(user, newPassword);
            if (!isValid)
                throw new ArgumentException();

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if(!result.Succeeded)
                throw new ArgumentException();
        }


        public async Task<T> GetByUserNameAsync(string userName)
        {
            return await GetObjects().SingleOrDefaultAsync(u => u.UserName.ToLower().Equals(userName.ToLower()));
        }

        protected virtual SecurityToken GetToken(User user)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Role, typeof(T).Name),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("Id", user.Id)
            };

            string tokenKey = _configuration["Token:Key"];
            byte[] tokenKeyEncoded = Encoding.ASCII.GetBytes(tokenKey);
            var key = new SymmetricSecurityKey(tokenKeyEncoded);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return token;
        }

        public virtual async Task<string> LoginAsync(string userName, string password)
        {
            var user = await this.GetByUserNameAsync(userName);

            if( user is null || !(await CheckPasswordAsync(password, user)).Succeeded)
                throw new ArgumentException("نام کاربری یا رمز عبور اشتباه است.");

            var token = GetToken(user);

            string stringToken = new JwtSecurityTokenHandler().WriteToken(token);

            return stringToken;
        }

        private async Task<SignInResult> CheckPasswordAsync(string password, T user)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, false);
        }

        public virtual async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
