using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.RevokeRefreshToken
{
    public class RevokeRefreshTokenCommandValidator: AbstractValidator<RevokeRefreshTokenCommand>
    {
        public RevokeRefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}
