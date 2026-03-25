using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public FactoryService(IUnitOfWork unitOfWork , IMapper mapper, UserManager<ApplicationUser> userManager
            ,IMediator mediator, IEnumerable<IConsumptionEvaluationStrategy> strategies)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mediator = mediator;
            _strategies = strategies;
        }
        public async Task<Result<CreateFactoryDto>> CreateFactory(CreateFactoryDto dto,string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Result<CreateFactoryDto>.Failure("User ID is missing.");
            }

            var factory = _mapper.Map<Factory>(dto);
            factory.ManagerId= userId;

            await _unitOfWork.Factories.AddAsync(factory);
            var result=await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                var manager = await _userManager.FindByIdAsync(userId);
                manager.FactoryId = factory.Id;

                var updateResult = await _userManager.UpdateAsync(manager);

                if (!updateResult.Succeeded)
                {
                    return Result<CreateFactoryDto>.Failure("Error happened while setting the manager");
                }

                return Result<CreateFactoryDto>.Success(dto);
            }

            return Result<CreateFactoryDto>.Failure("Failed to save the factory to the database.");
        }

        public async Task<Result<bool>> DeleteFactory(int id)
        {
            var factory = await _unitOfWork.Factories.Query.Include(f => f.Manager)
                .Include(f => f.Departments)
                .ThenInclude(d => d.Manager)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factory is null )
            {
                return Result<bool>.Failure("Factory not found.",404);
            }

            factory.Status = FactoryStatus.Deactivated;
             _unitOfWork.Factories.Update(factory);
            //تعطيل مدير المصنع
            if (factory.Manager != null)
            {
                factory.Manager.LockoutEnd = DateTimeOffset.MaxValue; // قفل الحساب
            }

            // تعطيل مديري الأقسام
            foreach (var dept in factory.Departments)
            {
                if (dept.Manager != null)
                {
                    dept.Manager.LockoutEnd = DateTimeOffset.MaxValue;
                }
            }

            var result = await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure("Failed to deactivate the factory in the database.");

        }

        public async Task<Result<List<FactoryDto>>> GetActiveFactories()
        {
            var activeFactories =await  _unitOfWork.Factories.Query.Include(f => f.Departments).Where(f => f.Status == FactoryStatus.Approved).ToListAsync();

            var activeFactoryDtos = _mapper.Map<List<FactoryDto>>(activeFactories);

            return Result<List<FactoryDto>>.Success(activeFactoryDtos);
        }

        public async Task<Result<List<FactoryDto>>> GetAllFactories()
        {
            var factories = await _unitOfWork.Factories.Query.Include(f=>f.Departments).ToListAsync();

            var factoryDtos = _mapper.Map<List<FactoryDto>>(factories);

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

            var currentTotalFactoryConsumption = await _unitOfWork.Departments.Query
               .Where(d => d.FactoryId == factoryId )
               .Select(d => d.ConsumptionLogs
                   .OrderByDescending(log => log.CapturedAt)
                   .Select(log => log.ConsumptionValue)
                   .FirstOrDefault()) // بياخد آخر قيمة للقسم ده، لو مفيش هتبقى 0
               .SumAsync();

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
            var factory = await _unitOfWork.Factories.Query.Include(f => f.Departments).FirstOrDefaultAsync(f => f.Id == id);

            if (factory is null)
            {
                return Result<FactoryDto>.Failure("Factory not found.",404);
            }

            _mapper.Map(dto, factory);
            _unitOfWork.Factories.Update(factory);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                var factoryDto = _mapper.Map<FactoryDto>(factory);
                return Result<FactoryDto>.Success(factoryDto);
            }

            return Result<FactoryDto>.Failure("Failed to update the factory in the database.");
        }
    }
}
