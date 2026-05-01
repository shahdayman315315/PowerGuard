using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.UpdateFactory
{
    public sealed record UpdateFactoryCommand(int FactoryId, string? Name, string? Location,string? Description) 
         : IRequest<Result<FactoryDto>>;
    
}
