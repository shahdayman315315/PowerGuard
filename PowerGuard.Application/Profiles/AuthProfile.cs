using AutoMapper;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Features.Auth.Password.ResetPassword;
using PowerGuard.Application.Features.Auth.Password.VerifyOtp;
using PowerGuard.Application.Features.Auth.RefreshToken;
using PowerGuard.Application.Features.Auth.Register;
using PowerGuard.Application.Features.Department.Commands.RegisterDepartmentManager;
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

            CreateMap<RegisterDepartmentManagerCommand,ApplicationUser>().
                ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.FullName))
                .ForSourceMember(src => src.DepartmentId, opt => opt.DoNotValidate());

            
        }
    }
}
