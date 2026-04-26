using AutoMapper;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Features.Auth.Register;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Profiles
{
    public class AuthProfile:Profile
    {
        public AuthProfile()
        {
            CreateMap<RegisterCommand, ApplicationUser>().ReverseMap();
            CreateMap<RegisterDto, RegisterCommandHandler>().ReverseMap();

            CreateMap<RegisterManagerDto,ApplicationUser>().
                ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.FullName))
                .ForSourceMember(src => src.DepartmentId, opt => opt.DoNotValidate());
        }
    }
}
