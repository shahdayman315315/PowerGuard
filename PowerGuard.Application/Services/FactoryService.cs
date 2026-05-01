using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Events;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Services
{
    public class FactoryService : IFactoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        private readonly IMemoryCache _memoryCache;

        public FactoryService(IUnitOfWork unitOfWork , IMapper mapper, UserManager<ApplicationUser> userManager
            ,IMediator mediator, IEnumerable<IConsumptionEvaluationStrategy> strategies, IMemoryCache memoryCache)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mediator = mediator;
            _strategies = strategies;
            _memoryCache = memoryCache;
        }
        public async Task<Result<CreateFactoryDto>> CreateFactory(CreateFactoryDto dto,string userId)
        {
            
        }

        public async Task<Result<bool>> DeleteFactory(int id)
        {
            

        }

        public async Task<Result<List<FactoryDto>>> GetActiveFactories()
        {
            string cacheKey = "ActiveFactoriesList";

            // محاولة الحصول على البيانات من الكاش
            if (!_memoryCache.TryGetValue(cacheKey, out List<FactoryDto> activeFactoryDtos))
            {
                var activeFactories =await  _unitOfWork.Factories.Query.Include(f => f.Departments).Where(f => f.Status == FactoryStatus.Approved).ToListAsync();

                activeFactoryDtos = _mapper.Map<List<FactoryDto>>(activeFactories);

                var cacheOptions = new MemoryCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromMinutes(20)) // لو محدش طلبها لمدة 20 دقايق تمسحها
               .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // تمسحها نهائياً بعد 30 دقيقة وتجددها

                _memoryCache.Set(cacheKey, activeFactoryDtos, cacheOptions);
            }

            return Result<List<FactoryDto>>.Success(activeFactoryDtos);
        }

        public async Task<Result<List<FactoryDto>>> GetAllFactories()
        {
            string cacheKey = "FactoriesList";

            // محاولة الحصول على البيانات من الكاش
            if (!_memoryCache.TryGetValue(cacheKey, out List<FactoryDto> factoryDtos))
            {
                var factories = await _unitOfWork.Factories.Query.Include(f=>f.Departments).ToListAsync();

                factoryDtos = _mapper.Map<List<FactoryDto>>(factories);

                var cacheOptions = new MemoryCacheEntryOptions()
              .SetSlidingExpiration(TimeSpan.FromMinutes(20)) // لو محدش طلبها لمدة 20 دقايق تمسحها
              .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // تمسحها نهائياً بعد 30 دقيقة وتجددها

                _memoryCache.Set(cacheKey, factoryDtos, cacheOptions);

            }

            return Result<List<FactoryDto>>.Success(factoryDtos);
        }

        public async Task<Result<FactoryDto>> GetFactoryById(int id)
        {
            var factory = await _unitOfWork.Factories.Query.Include(f=>f.Departments).FirstOrDefaultAsync(f => f.Id == id);

            if (factory is null)
            {
                return Result<FactoryDto>.Failure("Factory not found.");
            }

            var factoryDto = _mapper.Map<FactoryDto>(factory);

            return Result<FactoryDto>.Success(factoryDto);
        }

        public async Task<Result<List<FactoryDetailsDto>>> GetPendingFactories()
        {
            var pendingFactories =await  _unitOfWork.Factories.Query.Include(f => f.Manager).Where(f => f.Status == FactoryStatus.Pending).ToListAsync();

            var pendingFactoryDtos = _mapper.Map<List<FactoryDetailsDto>>(pendingFactories);

            return Result<List<FactoryDetailsDto>>.Success(pendingFactoryDtos);
        }

        public async Task<Result<bool>> UpdateConsumptionLimit(int factoryId, UpdateConsumptionLimitDto dto,string userId)
        {
            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId);

            if (factory is null)
            {
                return Result<bool>.Failure("Factory not found.",404);
            }

            var departmentConsumptions = await _unitOfWork.Departments.Query
                 .Where(d => d.FactoryId == factoryId)
                 .Select(d => d.ConsumptionLogs
                 .OrderByDescending(log => log.CapturedAt)
                 .Select(log => log.ConsumptionValue)
                 .FirstOrDefault())
                 .ToListAsync(); // اسحبيهم كـ List الأول

            var currentTotalFactoryConsumption = departmentConsumptions.Sum();

            var factoryStatus = ConsumptionStatus.Normal;
            if (factory.CurrentConsumptionLimit.HasValue)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(currentTotalFactoryConsumption, dto.NewLimit);

                    if (status > factoryStatus)
                    {
                        factoryStatus = status;
                    }
                }
            }


            if (factoryStatus != ConsumptionStatus.Normal)
            {
                await _mediator.Publish(new HighConsumptionDetectedEvent(currentTotalFactoryConsumption, factoryStatus, factoryId));
            }

            factory.CurrentConsumptionLimit = dto.NewLimit;
            
            var limitHistory = new LimitHistory
            {
                FactoryId = factoryId,
                LimitValue = (decimal)factory.CurrentConsumptionLimit,
                CreatedAt = DateTime.UtcNow,
                ActiveFrom = DateTime.UtcNow,
                SetBy=userId
            };

            await _unitOfWork.LimitHistories.AddAsync(limitHistory);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure("Failed to update the consumption limit in the database.");
        }



        public async Task<Result<FactoryDto>> UpdateFactory(int id, UpdateFactoryDto dto)
        {
        }
    }
}
