using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.UpdateConsumptionLimit
{
    public class UpdateConsumptionLimitCommandValidator:
        AbstractValidator<UpdateConsumptionLimitCommand>
    {
        public UpdateConsumptionLimitCommandValidator()
        {
            RuleFor(x => x.factoryId)
                .NotEmpty().WithMessage("Factory Id is required")
                .GreaterThan(0).WithMessage("Factory Id must be greater than zero");

            RuleFor(x => x.NewLimit)
                .NotEmpty().WithMessage("New Consumption Limit is required")
                .GreaterThan(0).WithMessage("New Consumption Limit must be greater than zero");


        }
    }
}
