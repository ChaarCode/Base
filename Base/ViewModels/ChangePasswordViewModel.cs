using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CharCode.Base.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        [StringLength(255, ErrorMessage = "رمز عبور باید حداقل 6 حرف باشد.", MinimumLength = 6)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "رمز عبور باید حداقل 6 حرف باشد.", MinimumLength = 6)]
        public string NewPassword { get; set; }
    }
}
