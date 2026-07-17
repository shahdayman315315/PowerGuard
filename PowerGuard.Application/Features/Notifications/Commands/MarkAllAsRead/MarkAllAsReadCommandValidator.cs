using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Notifications.Commands.MarkAllAsRead
{
    public class MarkAllAsReadCommandValidator:AbstractValidator<MarkAllAsReadCommand>
    {
        public MarkAllAsReadCommandValidator()
        {
            RuleFor(x => x.userId)
                .NotEmpty().WithMessage("User Id is required");
        }
    }
}
