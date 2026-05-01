using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.RefreshToken
{
    public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<AuthResultDto>>;

}
