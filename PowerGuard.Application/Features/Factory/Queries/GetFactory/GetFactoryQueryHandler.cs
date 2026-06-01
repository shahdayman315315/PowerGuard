using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.Queries.GetFactory
{
    public class GetFactoryCommandHandler : IRequestHandler<GetFactoryQuery, Result<FactoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetFactoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<FactoryDto>> Handle(GetFactoryQuery request, CancellationToken cancellationToken)
        {
            var factory = await _unitOfWork.Factories.Query.Include(f => f.Departments).FirstOrDefaultAsync(f => f.Id == request.FactoryId,cancellationToken);

            if (factory is null)
            {
                return Result<FactoryDto>.Failure("Factory not found.");
            }

            var factoryDto = _mapper.Map<FactoryDto>(factory);

            return Result<FactoryDto>.Success(factoryDto);
        }
    }
}
