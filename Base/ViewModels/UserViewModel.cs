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
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
