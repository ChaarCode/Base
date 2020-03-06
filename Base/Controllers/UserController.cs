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
using System.Linq;
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
        public UserController( IMapper mapper, TRepository repository
            )
            : base(repository, mapper)
        {
        }

        public override async Task<ActionResult<TUserViewModel>> InsertAsync([FromBody] TUserViewModel userViewModel)
        {
            userViewModel.Id = Guid.NewGuid().ToString();

            var user = _mapper.Map<TUser>(userViewModel);

            var insertedUser = await _repository.InsertAsync(user, userViewModel.Password);

            var result = _mapper.Map<TUserViewModel>(insertedUser);

            return Ok(result);
        }

        public override async Task<IActionResult> UpdateAsync(string id, [FromBody] TUserViewModel obj)
        {
            if (!ModelState.IsValid || !id.Equals(obj.Id))
                return BadRequest();

            var user = await _repository.GetAsync(obj.Id);

            _mapper.Map(obj, user);

            await _repository.UpdateAsync(obj.Id, user);

            if (!string.IsNullOrWhiteSpace(obj.Password))
            {
                await _repository.ChangePasswordAsync(user, obj.Password);
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LoginAsync([FromBody]LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _repository.LoginAsync(model.UserName, model.Password);

            var result = new { token };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePasswordAsync([FromBody]ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.User.Claims.Single(c => c.Type.Equals("Id")).Value;
            var user = await _repository.GetAsync(userId);

            await _repository.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            await _repository.LogoutAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> LogoutAsync()
        {
            await _repository.LogoutAsync();
            return Ok();
        }

        
    }
}
