using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Notifications.Queries.GetUserNotifications
{
    public class GetUserNotificationsQueryValidator:AbstractValidator<GetUserNotificationsQuery>
    {
        public GetUserNotificationsQueryValidator()
        {
            RuleFor(x => x.userId)
                .NotEmpty().WithMessage("User Id is required");

            RuleFor(x => x.pageNumber)
                .NotEmpty().WithMessage("Page Number is required")
                .GreaterThan(0).WithMessage("Page Number must be greater than zero");

            RuleFor(x => x.pageSize)
               .NotEmpty().WithMessage("Page Size is required")
               .GreaterThan(0).WithMessage("Page Size must be greater than zero");
        }
    }
}
