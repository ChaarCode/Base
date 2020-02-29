using AutoMapper;
using CharCode.Base.Abstraction;
using CharCode.Base.Models;
using CharCode.Base.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CharCode.Base.Controllers
{
    public abstract class UserController<TRepository , TUser, TUserViewModel> : BaseController<TRepository, TUser, TUserViewModel, string>
        where TRepository: IUserRepository<TUser>
        where TUser : User, new()
        where TUserViewModel : UserViewModel
    {
        protected readonly UserManager<User> _userManager;
        protected readonly SignInManager<User> _signInManager;
        protected readonly IConfiguration _configuration;
        public UserController(
            IMapper mapper,
            IConfiguration configuration,
            Microsoft.AspNetCore.Identity.UserManager<User> userManager,
            SignInManager<User> signInManager,
            TRepository repository
            )
            : base(repository, mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [AllowAnonymous]
        public override async Task<IActionResult> InsertAsync([FromBody] TUserViewModel userViewModel)
        {
            userViewModel.Id = Guid.NewGuid().ToString();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _mapper.Map<TUser>(userViewModel);
            var result = await _userManager.CreateAsync(user, userViewModel.Password);

            if (result.Succeeded)
                return Ok(user);

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return BadRequest();
        }

        public override async Task<IActionResult> UpdateAsync(string id, [FromBody] TUserViewModel obj)
        {
            if (!ModelState.IsValid || !id.Equals(obj.Id))
                return BadRequest();

            var user = await _repository.GetAsync(obj.Id);

            _mapper.Map(obj, user);

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return BadRequest();

            if (!string.IsNullOrWhiteSpace(obj.Password))
            {
                if (!(await _userManager.RemovePasswordAsync(user)).Succeeded)
                    return BadRequest();

                if (!(await _userManager.AddPasswordAsync(user, obj.Password)).Succeeded)
                    return BadRequest();
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LoginAsync([FromBody]LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await this._repository.GetByUserNameAsync(model.UserName);

            if (user == null)
                return BadRequest("نام کاربری یا رمز عبور اشتباه است.");

            var checkPwd = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!checkPwd.Succeeded)
                return BadRequest("نام کاربری یا رمز عبور اشتباه است.");

            var token = GetToken(user);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        private SecurityToken GetToken(User user)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Role, typeof(TUser).Name),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("Id", user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Token:Key"]));
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
    }
}
