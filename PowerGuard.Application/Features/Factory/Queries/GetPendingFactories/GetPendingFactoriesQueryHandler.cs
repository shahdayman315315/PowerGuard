using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.Queries.GetPendingFactories
{
    public class GetPendingFactoriesQueryHandler : IRequestHandler<GetPendingFactoriesQuery, Result<List<FactoryDetailsDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPendingFactoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Result<List<FactoryDetailsDto>>> Handle(GetPendingFactoriesQuery request, CancellationToken cancellationToken)
        {
            var pendingFactories = await _unitOfWork.Factories.Query.Include(f => f.Manager).Where(f => f.Status == FactoryStatus.Pending).ToListAsync(cancellationToken);

            var pendingFactoryDtos = _mapper.Map<List<FactoryDetailsDto>>(pendingFactories);

            return Result<List<FactoryDetailsDto>>.Success(pendingFactoryDtos);
        }
    }
}
