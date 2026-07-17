using MediatR;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.DeleteFactory
{
    public sealed record DeleteFactoryCommand(int FactoryId) : IRequest<Result<bool>>;
    
}
