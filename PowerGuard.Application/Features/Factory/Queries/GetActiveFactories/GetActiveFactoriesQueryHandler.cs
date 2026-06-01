using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Factory.Queries.GetActiveFactories
{
    public class GetActiveFactoriesQueryHandler : IRequestHandler<GetActiveFactoriesQuery, Result<List<FactoryDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;

        public GetActiveFactoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _memoryCache = memoryCache;
        }
        public async Task<Result<List<FactoryDto>>> Handle(GetActiveFactoriesQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = "ActiveFactoriesList";

            if (!_memoryCache.TryGetValue(cacheKey, out List<FactoryDto> activeFactoryDtos))
            {
                var activeFactories = await _unitOfWork.Factories.Query.Include(f => f.Departments).Where(f => f.Status == FactoryStatus.Approved).ToListAsync(cancellationToken);

                activeFactoryDtos = _mapper.Map<List<FactoryDto>>(activeFactories);

                var cacheOptions = new MemoryCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromMinutes(20)) 
               .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); 

                _memoryCache.Set(cacheKey, activeFactoryDtos, cacheOptions);
            }

            return Result<List<FactoryDto>>.Success(activeFactoryDtos);
        }
    }
}
