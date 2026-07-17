using MediatR;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Consumption.Commands.DeleteConsumptionLog
{
    public sealed record DeleteConsumptionLogCommand(int LogId):IRequest<Result<bool>>;
    
}
