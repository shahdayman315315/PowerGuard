using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.UpdateFactory
{
    public class UpdateFactoryCommandValidator:AbstractValidator<UpdateFactoryCommand>
    {
        public UpdateFactoryCommandValidator()
        {
            RuleFor(x=>x.Name)
                .NotEmpty().WithMessage("Factory Name is Required")
                .MaximumLength(100).WithMessage("Factory Name must not exceed 100 characters.");

            RuleFor(x => x.Location)
                .MaximumLength(200).WithMessage("Factory Location must not exceed 200 characters.");

            RuleFor(x=>x.Description)
                .MaximumLength(500).WithMessage("Factory Description must not exceed 500 characters.");
              
        }
    }
}
