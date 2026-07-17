using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.FactoryManagerDashboard.Queries.GetFactoryDailySummary
{
    public class GetFactoryDailySummaryQueryValidator:AbstractValidator<GetFactoryDailySummaryQuery>
    {
        public GetFactoryDailySummaryQueryValidator()
        {
            RuleFor(x => x.factoryId)
                .NotEmpty().WithMessage("Factory ID is required")
                .GreaterThan(0).WithMessage("Factory ID must be greater than 0");
        }
    }
}
