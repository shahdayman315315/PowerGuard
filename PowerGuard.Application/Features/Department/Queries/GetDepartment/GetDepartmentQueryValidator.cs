using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Queries.GetDepartment
{
    public class GetDepartmentQueryValidator:AbstractValidator<GetDepartmentQuery>
    {
        public GetDepartmentQueryValidator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("Department Id is required")
                .GreaterThan(0).WithMessage("Department Id must be greater than zero");

            
        }
    }
}
