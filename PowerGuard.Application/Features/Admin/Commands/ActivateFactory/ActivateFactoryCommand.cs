using MediatR;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Admin.Commands.activateFactory
{
    public sealed record ActivateFactoryCommand(int FactoryId):IRequest<Result<bool>>;
    
}
