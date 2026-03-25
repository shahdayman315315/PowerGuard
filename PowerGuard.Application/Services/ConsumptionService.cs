using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
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
using System.Transactions;

namespace PowerGuard.Application.Services
{
    public class ConsumptionService : IConsumptionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public ConsumptionService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, 
            IEnumerable<IConsumptionEvaluationStrategy> strategies, IMediator mediator,IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _strategies = strategies;
            _mediator = mediator;
            _mapper = mapper;
        }
        public async Task<Result<ConsumptionLogDto>> EnterConsumptionAsync(ConsumptionLogDto dto)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<ConsumptionLogDto>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId);

            if(factory is null)
            {
                return Result<ConsumptionLogDto>.Failure("Factory not found", 404);

            }

            var department = _unitOfWork.Departments.Query.FirstOrDefault(d => d.Id == dto.DepartmentId && d.FactoryId == factoryId);

            if (department is null)
            {
                return Result<ConsumptionLogDto>.Failure("department not found");
            }

            var currentTotalFactoryConsumption = await _unitOfWork.Departments.Query
                .Where(d => d.FactoryId == factoryId && d.Id !=department.Id)
                .Select(d => d.ConsumptionLogs
                    .OrderByDescending(log => log.CapturedAt)
                    .Select(log => log.ConsumptionValue)
                    .FirstOrDefault()) // بياخد آخر قيمة للقسم ده، لو مفيش هتبقى 0
                .SumAsync();

            var newTotalFactoryConsumption = currentTotalFactoryConsumption + dto.ConsumptionValue;

           

            var finalDepartmentStatus = ConsumptionStatus.Normal;
            if (department.CurrentConsumptionLimit.HasValue)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(dto.ConsumptionValue, department.CurrentConsumptionLimit ?? 0);

                    if (status > finalDepartmentStatus)
                    {
                        finalDepartmentStatus = status;
                    }
                }

            }

            var finalFactoryStatus= ConsumptionStatus.Normal;
            if (factory.CurrentConsumptionLimit.HasValue)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(newTotalFactoryConsumption, factory.CurrentConsumptionLimit ?? 0);

                    if (status > finalFactoryStatus)
                    {
                        finalFactoryStatus = status;
                    }
                }
            }
            

            var log = new ConsumptionLog
            {
                ConsumptionValue = dto.ConsumptionValue,
                DepartmentId = dto.DepartmentId,
                Status = finalDepartmentStatus,
                CapturedAt = dto.CapturedAt
            };

            await _unitOfWork.ConsumptionLogs.AddAsync(log);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result <= 0)
            {
                return Result<ConsumptionLogDto>.Failure("Error Adding The log in the data base");
            }

            if(finalDepartmentStatus != ConsumptionStatus.Normal ||finalFactoryStatus !=ConsumptionStatus.Normal)
            {
                await _mediator.Publish(new HighConsumptionDetectedEvent( log.ConsumptionValue, finalFactoryStatus, factoryId, finalDepartmentStatus, department.Name,department.Id, log.Id));
            }

            var logDto=_mapper.Map<ConsumptionLogDto>(log);    
            return Result<ConsumptionLogDto>.Success(logDto);
        }
    }
}
