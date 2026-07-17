using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Consumption.Queries.GetConsumptionLogs
{
    public class GetConsumptionLogsQueryValidator:AbstractValidator<GetConsumptionLogsQuery>
    {
        public GetConsumptionLogsQueryValidator()
        {
            RuleFor(x => x.departmentId)
                .GreaterThan(0).WithMessage("Department ID must be a positive integer.")
                .NotEmpty().WithMessage("Department ID is required.");

            RuleFor(x => x.pageNumber)
                .GreaterThan(0).WithMessage("Page number must be a positive integer.")
                .NotEmpty().WithMessage("Page number is required.");

            RuleFor(x => x.pageSize)
                .GreaterThan(0).WithMessage("Page size must be a positive integer.")
                .NotEmpty().WithMessage("Page size is required.");
        }
    }
}
