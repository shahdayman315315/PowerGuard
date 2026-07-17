using MediatR;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Admin.Commands.ReviewFactory
{
    public sealed record ReviewFactoryCommand( int FactoryId, bool IsApproved , string? AdminRemarks ):
        IRequest<Result<bool>>;
    
}
