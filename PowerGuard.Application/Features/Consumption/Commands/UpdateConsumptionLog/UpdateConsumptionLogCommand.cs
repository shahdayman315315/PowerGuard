using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Consumption.Commands.UpdateConsumptionLog
{
    public sealed record  UpdateConsumptionLogCommand(int LogId, decimal ConsumptionValue) 
         : IRequest<Result<ConsumptionLogDto>>;
    
}
