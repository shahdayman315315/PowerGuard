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

namespace PowerGuard.Application.Features.Factory.Queries.GetAllFactories
{
    public class GetAllFactoriesQueryHandler : IRequestHandler<GetAllFactoriesQuery, Result<List<FactoryDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        public GetAllFactoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }
        
        public async Task<Result<List<FactoryDto>>> Handle(GetAllFactoriesQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = "FactoriesList";

            var factoryDtos = await _cache.GetAsync<List<FactoryDto>>(cacheKey);

            if (factoryDtos == null)
            {
                var factories = await _unitOfWork.Factories.Query.Include(f => f.Departments).ToListAsync(cancellationToken);

                factoryDtos = _mapper.Map<List<FactoryDto>>(factories);


                await _cache.SetAsync(cacheKey, factoryDtos, TimeSpan.FromMinutes(60), TimeSpan.FromMinutes(30));
            }

            return Result<List<FactoryDto>>.Success(factoryDtos);

        }
    }
}
