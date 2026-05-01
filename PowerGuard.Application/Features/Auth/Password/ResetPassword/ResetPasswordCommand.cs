using MediatR;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.Password.ResetPassword
{
    public sealed record ResetPasswordCommand(string Email, string resetToken, string NewPassword) : IRequest<Result<bool>>
    {
    }
}
