using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.Queries.GetPendingFactories
{
    public sealed record GetPendingFactoriesQuery:IRequest<Result<List<FactoryDetailsDto>>>;

}
