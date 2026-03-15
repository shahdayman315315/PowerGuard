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
    public class DepartmentProfile:Profile
    {
        public DepartmentProfile()
        {
            CreateMap<CreateDepartmentDto, Department>();
            CreateMap<Department, DepartmentDto>().ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager.UserName??" No anager Assigned"));
            CreateMap<UpdateDepartmentDto, Department>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
