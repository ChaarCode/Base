﻿using CharCode.Base.Abstraction;
using CharCode.Base.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharCode.Base.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationRepository repository;

        public AuthenticationController(IAuthenticationRepository authenticationRepository)
        {
            this.repository = authenticationRepository ?? throw new ArgumentNullException(nameof(authenticationRepository));
        }

        [AllowAnonymous]
        [HttpPost]
        public virtual async Task<IActionResult> LoginAsync([FromBody]LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var token = await this.repository.LoginAsync(model.UserName, model.Password);

                var result = new { token };

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePasswordAsync([FromBody]ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.User.Claims.Single(c => c.Type.Equals("Id")).Value;

            await this.repository.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

            await this.repository.LogoutAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> LogoutAsync()
        {
            await this.repository.LogoutAsync();
            return Ok();
        }
    }
}
