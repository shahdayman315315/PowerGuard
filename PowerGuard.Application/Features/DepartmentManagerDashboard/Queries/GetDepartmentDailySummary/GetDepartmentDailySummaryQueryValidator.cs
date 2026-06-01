using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.DepartmentManagerDashboard.Queries.GetDepartmentDailySummary
{
    public class GetDepartmentDailySummaryQueryValidator:AbstractValidator<GetDepartmentDailySummaryQuery>
    {
        public GetDepartmentDailySummaryQueryValidator()
        {
            RuleFor(x => x.departmentId)
                .NotEmpty().WithMessage("Department ID is required")
                .GreaterThan(0).WithMessage("Department ID must be greater than 0");
        }
    }
}
