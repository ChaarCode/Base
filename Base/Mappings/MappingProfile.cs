using AutoMapper;
using CharCode.Base.Models;
using CharCode.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CharCode.Bringo.Models.Mappings
{
    public class BaseMappingProfile : Profile
    {
        public BaseMappingProfile()
        {
            CreateMap<User, UserViewModel>().ReverseMap();
        }
    }
}
