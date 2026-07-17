using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Consumption.Commands.EnterConsumption
{
    public class EnterConsumptionCommandValidator:
        AbstractValidator<EnterConsumptionCommand>
    {
        public EnterConsumptionCommandValidator()
        {
            RuleFor(x => x.ConsumptionValue)
                .GreaterThanOrEqualTo(0).WithMessage("Consumption value must be non-negative.")
                .NotEmpty().WithMessage("Consumption value is required.");

            
            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Department ID must be a positive integer.")
                .NotEmpty().WithMessage("Department ID is required.");


        }
    }
}
