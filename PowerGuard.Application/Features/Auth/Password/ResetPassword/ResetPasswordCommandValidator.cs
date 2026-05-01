using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.Password.ResetPassword
{
    public class ResetPasswordCommandValidator:
        AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.resetToken)
                .NotEmpty().WithMessage("Reset token is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.");
        }
    }
}
