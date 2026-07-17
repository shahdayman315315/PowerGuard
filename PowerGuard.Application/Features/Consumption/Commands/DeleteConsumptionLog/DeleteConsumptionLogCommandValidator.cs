using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Consumption.Commands.DeleteConsumptionLog
{
    public class DeleteConsumptionLogCommandValidator:AbstractValidator<DeleteConsumptionLogCommand>
    {
        public DeleteConsumptionLogCommandValidator()
        {
            RuleFor(x => x.LogId)
                .GreaterThan(0).WithMessage("Log ID must be a positive integer.")
                .NotEmpty().WithMessage("Log ID is required.");
        }
    }
}
