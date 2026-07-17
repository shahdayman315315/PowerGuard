using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Queries.GetAllDepartments
{
    public class GetAllDepartmentsQueryValidator:AbstractValidator<GetAllDepartmentsQuery>
    {
        public GetAllDepartmentsQueryValidator()
        {
            RuleFor(x => x.FactoryId)
                .NotEmpty().WithMessage("Factory Id is required")
                .GreaterThan(0).WithMessage("Factory Id must be greater than zero");
        }
    }
}
