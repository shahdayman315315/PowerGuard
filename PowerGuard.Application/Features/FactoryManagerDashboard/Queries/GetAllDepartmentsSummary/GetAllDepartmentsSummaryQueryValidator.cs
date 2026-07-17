using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.FactoryManagerDashboard.Queries.GetAllDepartmentsSummary
{
    public class GetAllDepartmentsSummaryQueryValidator:AbstractValidator<GetAllDepartmentsSummaryQuery>
    {
            public GetAllDepartmentsSummaryQueryValidator()
            {
                RuleFor(x => x.factoryId)
                    .NotEmpty().WithMessage("Factory ID is required")
                    .GreaterThan(0).WithMessage("Factory ID must be greater than 0");
        }
    }
}
