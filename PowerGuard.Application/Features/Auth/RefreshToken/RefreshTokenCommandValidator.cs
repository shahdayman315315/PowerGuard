using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.RefreshToken
{
    public class RefreshTokenCommandValidator: AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
               .NotEmpty().WithMessage("Refresh token is required.");

            RuleFor(x => x.AccessToken)
               .NotEmpty().WithMessage("Access token is required.");
        }
    }
}
