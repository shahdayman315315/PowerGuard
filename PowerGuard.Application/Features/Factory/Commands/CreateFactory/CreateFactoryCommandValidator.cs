using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.CreateFactory
{
    public class CreateFactoryCommandValidator:AbstractValidator<CreateFactoryCommand>
    {
        public CreateFactoryCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Factory name is required.")
                .MaximumLength(100).WithMessage("Factory name must not exceed 100 characters.");

            RuleFor(x => x.Location)
                .MaximumLength(200).WithMessage("Factory location must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Factory description must not exceed 500 characters.");

            RuleFor(x=>x.CurrentConsumptionLimit)
                .GreaterThanOrEqualTo(0).WithMessage("Current consumption limit must be non-negative.");
        }
    }
}
