using FluentValidation;
using PowerGuard.Application.Features.Admin.Commands.activateFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Admin.Commands.ActivateFactory
{
    public class ActivateFactoryCommandValidator:AbstractValidator<ActivateFactoryCommand>
    {
        public ActivateFactoryCommandValidator()
        {
            RuleFor(x => x.FactoryId)
                .NotEmpty().WithMessage("Factory Id is required")
                .GreaterThan(0).WithMessage("Factory Id must be greater than zero");
        }
    }
}
