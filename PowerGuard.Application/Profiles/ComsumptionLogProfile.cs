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
    public class ComsumptionLogProfile:Profile
    {
        public ComsumptionLogProfile()
        {
            CreateMap<ConsumptionLog,ConsumptionLogDto>().ReverseMap();
        }
    }
}
