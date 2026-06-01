using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Notifications.Queries.GetUnReadCount
{
    public class GetUnReadCountQueryValidator:AbstractValidator<GetUnReadCountQuery>
    {
        public GetUnReadCountQueryValidator()
        {
            RuleFor(x => x.userId)
                .NotEmpty().WithMessage("User Id is required");

        }
    }
}
