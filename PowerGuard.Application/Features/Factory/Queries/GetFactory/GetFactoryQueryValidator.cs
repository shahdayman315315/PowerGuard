using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.Queries.GetFactory
{
    public class GetFactoryQueryValidator:
         AbstractValidator<GetFactoryQuery>
    {
        public GetFactoryQueryValidator()
        {
            RuleFor(x => x.FactoryId)
                .GreaterThan(0).WithMessage("Factory ID must be a positive integer.")
                .NotEmpty().WithMessage("Factory ID is required.");
        }
    }
}
