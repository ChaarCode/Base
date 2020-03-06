using CharCode.Base.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CharCode.Base.ViewModels
{
    public class UserViewModel : IViewModel<string>
    {
        public string Id { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9._]+$", ErrorMessage = "نام کاربری انتخابی معتبر نمی‌باشد")]
        public virtual string UserName { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "رمز عبور باید حداقل 6 حرف باشد.", MinimumLength = 6)]
        public virtual string Password { get; set; }
    }
}
