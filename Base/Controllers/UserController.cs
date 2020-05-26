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
    public abstract class UserController<TRepository, TUser, TUserViewModel> : BaseController<TRepository, TUser, TUserViewModel, string>
        where TRepository : IUserRepository<TUser>
        where TUser : User, new()
        where TUserViewModel : UserViewModel
    {
        private readonly IAuthenticationRepository authenticationRepository;

        public UserController(IMapper mapper, TRepository repository, IAuthenticationRepository authenticationRepository)
            : base(repository, mapper)
        {
            this.authenticationRepository = authenticationRepository ?? throw new ArgumentNullException(nameof(authenticationRepository));
        }

        public override async Task<ActionResult<TUserViewModel>> InsertAsync([FromBody] TUserViewModel userViewModel)
        {
            try
            {
                userViewModel.Id = Guid.NewGuid().ToString();

                var user = mapper.Map<TUser>(userViewModel);

                var insertedUser = await repository.InsertAsync(user, userViewModel.Password);

                var result = mapper.Map<TUserViewModel>(insertedUser);

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        public override async Task<IActionResult> UpdateAsync(string id, [FromBody] TUserViewModel obj)
        {
            if (!ModelState.IsValid || !id.Equals(obj.Id))
                return BadRequest();

            var user = await repository.GetAsync(obj.Id);

            mapper.Map(obj, user);

            await repository.UpdateAsync(obj.Id, user);

            if (!string.IsNullOrWhiteSpace(obj.Password))
            {
                await this.authenticationRepository.ChangePasswordAsync(user, obj.Password);
            }

            return Ok();
        }


    }
}
