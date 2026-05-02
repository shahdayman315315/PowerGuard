using AutoMapper;
using PowerGuard.Application.Dtos;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Profiles
{
    public class FactoryProfile:Profile
    {
        public FactoryProfile()
        {
            CreateMap<Factory,CreateFactoryDto>().ReverseMap();
            CreateMap<Factory, FactoryDto>()
                .ForMember(dest => dest.DepartmentsCount,
                           opt => opt.MapFrom(src => src.Departments != null ? src.Departments.Count : 0));
            CreateMap<UpdateFactoryDto,Factory >()
                            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Factory, FactoryDetailsDto>()
                .ForMember(dest => dest.ManagerName,
                           opt => opt.MapFrom(src => src.Manager.UserName))
                .ForMember(dest => dest.ManagerEmail,
                             opt => opt.MapFrom(src => src.Manager.Email));

            
        }
    }
}
