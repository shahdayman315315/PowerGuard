using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Consumption.Commands.EnterConsumption
{
    public sealed record EnterConsumptionCommand(decimal ConsumptionValue, DateTime CapturedAt, int DepartmentId) 
        : IRequest<Result<ConsumptionLogDto>>;
    
}
