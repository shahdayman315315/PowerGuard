using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Consumption.Commands.UpdateConsumptionLog
{
    public class UpdateConsumptionLogCommandValidator: AbstractValidator<UpdateConsumptionLogCommand>
    {
        public UpdateConsumptionLogCommandValidator()
        {
            RuleFor(x => x.ConsumptionValue)
                .NotEmpty().WithMessage("Value is Required")
                .GreaterThanOrEqualTo(0).WithMessage("Consumption value must be non-negative.");

            RuleFor(x => x.LogId)
                .GreaterThan(0).WithMessage("Log ID must be a positive integer.")
                .NotEmpty().WithMessage("Log ID is required.");

        }
    }
}
