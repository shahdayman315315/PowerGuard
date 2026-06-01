using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Commands.UpdateConsumptionLimit
{
    public class UpdateConsumptionLimitCommandValidator
        :AbstractValidator<UpdateConsumptionLimitCommand>
    {
        public UpdateConsumptionLimitCommandValidator()
        {
            RuleFor(x => x.NewLimit)
                .NotEmpty().WithMessage("New consumption limit is required.")
                .GreaterThan(0).WithMessage("New consumption limit must be greater than zero.");

            RuleFor(x => x.departmentId)
                .NotEmpty().WithMessage("Department ID is required.")
                .GreaterThan(0).WithMessage("Department ID must be greater than zero.");

             RuleFor(x => x.userId)
                .NotEmpty().WithMessage("User ID is required.");
        }
    }
}
