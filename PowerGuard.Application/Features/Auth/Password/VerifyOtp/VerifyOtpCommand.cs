using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.Password.VerifyOtp
{
    public sealed record VerifyOtpCommand(string Email, string Otp) : IRequest<Result<AuthResultDto>>;

}
