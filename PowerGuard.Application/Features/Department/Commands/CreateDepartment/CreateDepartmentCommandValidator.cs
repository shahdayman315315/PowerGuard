using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Commands.CreateDepartment
{
    public class CreateDepartmentCommandValidator:
        AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Department name is required.")
                .MaximumLength(100).WithMessage("Department name must not exceed 100 characters.");

            RuleFor(x => x.CurrentConsumptionLimit)
                .GreaterThanOrEqualTo(0).WithMessage("Current consumption limit must be greater than or equal to zero.");

            RuleFor(x => x.OperatingHours)
                .NotEmpty().WithMessage("Operating hours are required.");

            RuleFor(x => x.Description)
               .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        }
       
    }
}
