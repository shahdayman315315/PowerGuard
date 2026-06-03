using MediatR;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Commands.UpdateConsumptionLimit
{
    public sealed record UpdateConsumptionLimitCommand(int departmentId, decimal NewLimit) : 
        IRequest<Result<bool>>;
    
}
