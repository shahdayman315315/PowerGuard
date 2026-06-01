using FluentValidation;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Admin.Commands.ReviewFactory
{
    public class ReviewFactoryCommandValidator:AbstractValidator<ReviewFactoryCommand>
    {
        public ReviewFactoryCommandValidator()
        {
            RuleFor(x=> x.FactoryId)
                .NotEmpty().WithMessage("Factory Id is required")
                .GreaterThan(0).WithMessage("Factory Id must be greater than zero");

            RuleFor(x => x.AdminRemarks)
                .MaximumLength(500).WithMessage("Admin Remarks must be less than 500 chars");

            RuleFor(x=>x.IsApproved)
                .NotEmpty().WithMessage("Approval status is required");

        }
    }
}
