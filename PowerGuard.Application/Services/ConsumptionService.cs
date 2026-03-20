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

namespace PowerGuard.Application.Services
{
    public class ConsumptionService : IConsumptionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        private readonly IMediator _mediator;  
        public ConsumptionService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, IEnumerable<IConsumptionEvaluationStrategy> strategies, IMediator mediator)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _strategies = strategies;
            _mediator = mediator;
        }
        public async Task<Result<ConsumptionLogDto>> EnterConsumptionAsync(ConsumptionLogDto dto)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<ConsumptionLogDto>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var department = _unitOfWork.Departments.Query.FirstOrDefault(d => d.Id == dto.DepartmentId && d.FactoryId == factoryId);

            if (department is null)
            {
                return Result<ConsumptionLogDto>.Failure("department not found");
            }

            var finalStatus = ConsumptionStatus.Normal;
            foreach(var strategy in _strategies)
            {
                var status = strategy.Evaluate(dto.ConsumptionValue, department.CurrentConsumptionLimit??0);

                if(status> finalStatus)
                {
                    finalStatus = status;
                }
            }

            var log = new ConsumptionLog
            {
                ConsumptionValue = dto.ConsumptionValue,
                DepartmentId = dto.DepartmentId,
                Status = finalStatus,
                CapturedAt = dto.CapturedAt
            };

            await _unitOfWork.ConsumptionLogs.AddAsync(log);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result <= 0)
            {
                return Result<ConsumptionLogDto>.Failure("Error Adding The log in the data base");
            }

            if(finalStatus != ConsumptionStatus.Normal)
            {
                await _mediator.Publish(new HighConsumptionDetectedEvent(log.Id, log.ConsumptionValue, department.Name, finalStatus.ToString()));
            }
            throw new NotImplementedException();
        }
    }
}
