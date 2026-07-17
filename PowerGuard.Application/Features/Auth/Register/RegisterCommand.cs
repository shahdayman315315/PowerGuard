using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.Register
{
    public sealed record RegisterCommand(string UserName,
                                         string Email,
                                         string Password,
                                         string ConfirmPassword,
                                         string PhoneNumber,
                                         int? FactoryId) :IRequest<Result<AuthResultDto>>;
    
}
