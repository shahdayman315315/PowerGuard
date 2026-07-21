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

namespace PowerGuard.Application.Features.Factory.Queries.GetActiveFactories
{
    public class GetActiveFactoriesQueryHandler : IRequestHandler<GetActiveFactoriesQuery, Result<List<FactoryDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        public GetActiveFactoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }
        
        public async Task<Result<List<FactoryDto>>> Handle(GetActiveFactoriesQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = "ActiveFactoriesList";

             var activeFactoryDtos = await _cache.GetAsync<List<FactoryDto>>(cacheKey);

            if (activeFactoryDtos == null)
            {
                var activeFactories = await _unitOfWork.Factories.Query.Include(f => f.Departments).Where(f => f.Status == FactoryStatus.Approved).ToListAsync(cancellationToken);

                activeFactoryDtos = _mapper.Map<List<FactoryDto>>(activeFactories);

                await _cache.SetAsync(cacheKey, activeFactoryDtos, TimeSpan.FromMinutes(60), TimeSpan.FromMinutes(30)); 

            }

            return Result<List<FactoryDto>>.Success(activeFactoryDtos);
        }
    }
}
