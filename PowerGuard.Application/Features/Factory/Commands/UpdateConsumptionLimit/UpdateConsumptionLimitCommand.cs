using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.UpdateConsumptionLimit
{
    public sealed record UpdateConsumptionLimitCommand(int factoryId, decimal NewLimit)
        :IRequest<Result<bool>>;

}
