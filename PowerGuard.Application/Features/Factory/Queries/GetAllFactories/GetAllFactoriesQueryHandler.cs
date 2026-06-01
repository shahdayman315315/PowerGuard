using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _memoryCache;

        public GetAllFactoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _memoryCache = memoryCache;
        }
        public async Task<Result<List<FactoryDto>>> Handle(GetAllFactoriesQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = "FactoriesList";

            if (!_memoryCache.TryGetValue(cacheKey, out List<FactoryDto> factoryDtos))
            {
                var factories = await _unitOfWork.Factories.Query.Include(f => f.Departments).ToListAsync(cancellationToken);

                factoryDtos = _mapper.Map<List<FactoryDto>>(factories);

                var cacheOptions = new MemoryCacheEntryOptions()
              .SetSlidingExpiration(TimeSpan.FromMinutes(20)) 
              .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); 

                _memoryCache.Set(cacheKey, factoryDtos, cacheOptions);

            }

            return Result<List<FactoryDto>>.Success(factoryDtos);
        }
    }
}
