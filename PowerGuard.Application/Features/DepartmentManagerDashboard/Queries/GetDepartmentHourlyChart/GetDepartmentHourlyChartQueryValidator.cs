using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.DepartmentManagerDashboard.Queries.GetDepartmentHourlyChart
{
    public class GetDepartmentHourlyChartQueryValidator:AbstractValidator<GetDepartmentHourlyChartQuery>
    {
        public GetDepartmentHourlyChartQueryValidator()
        {
            RuleFor(x => x.departmentId)
                .NotEmpty().WithMessage("Department ID is required")
                .GreaterThan(0).WithMessage("Department ID must be greater than 0");
        }
    }
}
