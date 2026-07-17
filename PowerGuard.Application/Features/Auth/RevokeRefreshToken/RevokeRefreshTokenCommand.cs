using MediatR;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.RevokeRefreshToken
{
    public sealed record RevokeRefreshTokenCommand(string RefreshToken) : IRequest<Result<bool>>;
    

}
