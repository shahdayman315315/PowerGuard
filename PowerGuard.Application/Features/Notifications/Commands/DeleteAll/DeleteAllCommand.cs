using MediatR;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Notifications.Commands.DeleteAll
{
    public sealed record DeleteAllCommand(string userId):IRequest<Result<bool>>;
    
}
