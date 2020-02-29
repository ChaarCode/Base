using CharCode.Base.Abstraction;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CharCode.Base.Models
{
    public abstract class User : IdentityUser, IModel<string>
    {
    }
}
