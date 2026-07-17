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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Consumption.Commands.UpdateConsumptionLog
{
    public class UpdateConsumptionLogCommandHandler :
        IRequestHandler<UpdateConsumptionLogCommand, Result<ConsumptionLogDto>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateConsumptionLogCommandHandler(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, 
            IEnumerable<IConsumptionEvaluationStrategy> strategies, IMediator mediator, IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _strategies = strategies;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<Result<ConsumptionLogDto>> Handle(UpdateConsumptionLogCommand request, CancellationToken cancellationToken)
        {
            var log = await _unitOfWork.ConsumptionLogs.Query.Include(l => l.Department).FirstOrDefaultAsync(l => l.Id == request.LogId);

            if (log is null)
            {
                return Result<ConsumptionLogDto>.Failure("Log is not found");
            }

            log.ConsumptionValue = request.ConsumptionValue;

            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<ConsumptionLogDto>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId, cancellationToken);

            if (factory is null)
            {
                return Result<ConsumptionLogDto>.Failure("Factory not found", 404);

            }


            var date = log.CapturedAt.Date;

            var previousDayTotalFactoryConsumption = await _unitOfWork.Departments.Query
                .AsNoTracking()
                .Where(d => d.FactoryId == factoryId)
                .Select(d => d.ConsumptionLogs.Where(l => l.CapturedAt < date)
                    .OrderByDescending(log => log.CapturedAt)
                    .Select(log => log.ConsumptionValue)
                    .FirstOrDefault()) // بياخد آخر قيمة للقسم ده، لو مفيش هتبقى 0
                .SumAsync();

            var currentTotalFactoryConsumption = await _unitOfWork.Departments.Query
                .Where(d => d.FactoryId == factoryId && d.Id != log.DepartmentId)
                .Select(d => d.ConsumptionLogs.Where(l => l.CapturedAt >= date)
                    .OrderByDescending(log => log.CapturedAt)
                    .Select(log => log.ConsumptionValue)
                    .FirstOrDefault()) // بياخد آخر قيمة للقسم ده، لو مفيش هتبقى 0
                .SumAsync();

            var newTotalFactoryConsumption = currentTotalFactoryConsumption + request.ConsumptionValue;

            var factoryConsumptionForToday = Math.Max(0, newTotalFactoryConsumption - previousDayTotalFactoryConsumption);

            var previousDayDepartmentConsumption = await _unitOfWork.ConsumptionLogs.Query
                .AsNoTracking()
                .Where(l => l.DepartmentId == log.DepartmentId && l.CapturedAt < date)
                .OrderByDescending(l => l.CapturedAt)
                .FirstOrDefaultAsync();


            var departmentConsumptionForToday = Math.Max(0, request.ConsumptionValue - (previousDayDepartmentConsumption == null ? 0 : previousDayDepartmentConsumption.ConsumptionValue));

            var finalDepartmentStatus = ConsumptionStatus.Normal;
            if (log.Department.CurrentConsumptionLimit.HasValue)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(departmentConsumptionForToday, log.Department.CurrentConsumptionLimit ?? 0);

                    if (status > finalDepartmentStatus)
                    {
                        finalDepartmentStatus = status;
                    }
                }

            }


            var finalFactoryStatus = ConsumptionStatus.Normal;
            if (factory.CurrentConsumptionLimit.HasValue)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(factoryConsumptionForToday, factory.CurrentConsumptionLimit ?? 0);

                    if (status > finalFactoryStatus)
                    {
                        finalFactoryStatus = status;
                    }
                }
            }

            log.Status = finalDepartmentStatus;

            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result <= 0)
            {
                return Result<ConsumptionLogDto>.Failure("Error Updating The log in the data base");
            }

            if (finalDepartmentStatus != ConsumptionStatus.Normal || finalFactoryStatus != ConsumptionStatus.Normal)
            {
                await _mediator.Publish(new HighConsumptionDetectedEvent(departmentConsumptionForToday, finalFactoryStatus, factoryId, finalDepartmentStatus, log.Department.Name, log.Department.Id, log.Id));
            }

            var logDto = _mapper.Map<ConsumptionLogDto>(log);

            return Result<ConsumptionLogDto>.Success(logDto);
        }
    }
}
