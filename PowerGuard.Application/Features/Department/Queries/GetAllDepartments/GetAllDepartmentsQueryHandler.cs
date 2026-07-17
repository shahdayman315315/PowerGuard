using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Queries.GetAllDepartments
{
    public class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, Result<IEnumerable<DepartmentDto>>>
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllDepartmentsQueryHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor
            , IMemoryCache cache, IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Result<IEnumerable<DepartmentDto>>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
        {
            int factoryId;

            if (request.FactoryId.HasValue)
            {
                factoryId=request.FactoryId.Value;
            }

            else
            {
                var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

                if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryIdfromCliam))
                {
                    return Result<IEnumerable<DepartmentDto>>.Failure("Factory ID claim is missing or invalid.", 400);
                }

                factoryId = factoryIdfromCliam;
            }

            string cacheKey = $"Departments-Factory: {factoryId}";

            if (!_cache.TryGetValue(cacheKey, out IEnumerable<DepartmentDto> departmentsDtos))
            {
                departmentsDtos = await _unitOfWork.Departments.Query.Where(d => d.FactoryId == factoryId).ProjectTo<DepartmentDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);

                var cacheOptions = new MemoryCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromMinutes(30))
               .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(cacheKey, departmentsDtos, cacheOptions);
            }

            return Result<IEnumerable<DepartmentDto>>.Success(departmentsDtos);
        }
    }
}
