using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Notifications.Commands.MarkAsRead
{
    public class MarkAsReadCommandValidator:AbstractValidator<MarkAsReadCommand>
    {
        public MarkAsReadCommandValidator()
        {
            RuleFor(x => x.notificationId)
                .NotEmpty().WithMessage("Notification Id ie required")
                .GreaterThan(0).WithMessage("Notification Id Greater than zero");

            RuleFor(x => x.userId)
                .NotEmpty().WithMessage("User Id is required");
        }
    }
}
