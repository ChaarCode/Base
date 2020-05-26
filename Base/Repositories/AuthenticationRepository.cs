using CharCode.Base.Abstraction;
using CharCode.Base.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CharCode.Base.Repositories
{
    public class AuthenticationRepository<TDbContext> : IAuthenticationRepository where TDbContext : DbContext
    {
        protected readonly UserManager<User> userManager;
        protected readonly SignInManager<User> signInManager;
        protected readonly IConfiguration configuration;
        protected TDbContext Context { get; private set; }

        public virtual async Task ChangePasswordAsync(User user, string password)
        {
            await userManager.RemovePasswordAsync(user);
            await userManager.AddPasswordAsync(user, password);
        }

        public virtual async Task ChangePasswordAsync(string userId, string password)
        {
            var user = await this.userManager.FindByIdAsync(userId);

            await ChangePasswordAsync(user, password);
        }

        public virtual async Task ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            var isValid = await userManager.CheckPasswordAsync(user, newPassword);
            if (!isValid)
                throw new ArgumentException();

            var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
                throw new ArgumentException();
        }

        public virtual async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await this.userManager.FindByIdAsync(userId);

            await ChangePasswordAsync(user, currentPassword, newPassword);
        }


        public async Task<User> GetByUserNameAsync(string userName)
        {
            return await Context.Set<User>().SingleOrDefaultAsync(u => u.UserName.ToLower().Equals(userName.ToLower()));
        }

        protected virtual SecurityToken GetToken(User user)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Role, user.GetType().Name),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("Id", user.Id)
            };

            string tokenKey = configuration["Token:Key"];
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

            if (user is null || !(await CheckPasswordAsync(password, user)).Succeeded)
                throw new ArgumentException("نام کاربری یا رمز عبور اشتباه است.");

            var token = GetToken(user);

            string stringToken = new JwtSecurityTokenHandler().WriteToken(token);

            return stringToken;
        }

        private async Task<SignInResult> CheckPasswordAsync(string password, User user)
        {
            return await signInManager.CheckPasswordSignInAsync(user, password, false);
        }

        public virtual async Task LogoutAsync()
        {
            await signInManager.SignOutAsync();
        }
    }
}
