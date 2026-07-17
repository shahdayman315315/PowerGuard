using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Commands.DeleteDepartment
{
    public class DeleteDepartmentCommandValidator:
        AbstractValidator<DeleteDepartmentCommand>
    {
        public DeleteDepartmentCommandValidator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("Department ID is required.")
                .GreaterThan(0).WithMessage("Department ID must be greater than zero.");
            
        }
    }
}
