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

namespace PowerGuard.Application.Features.Consumption.Commands.EnterConsumption
{
    public class EnterConsumptionCommandHandler : IRequestHandler<EnterConsumptionCommand, Result<ConsumptionLogDto>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public EnterConsumptionCommandHandler(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork,
            IEnumerable<IConsumptionEvaluationStrategy> strategies, IMediator mediator, IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _strategies = strategies;
            _mediator = mediator;
            _mapper = mapper;
        }
        public async Task<Result<ConsumptionLogDto>> Handle(EnterConsumptionCommand request, CancellationToken cancellationToken)
        {
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

            var department = _unitOfWork.Departments.Query.FirstOrDefault(d => d.Id == request.DepartmentId && d.FactoryId == factoryId);

            if (department is null)
            {
                return Result<ConsumptionLogDto>.Failure("department not found");
            }

            var date = DateTime.UtcNow.Date;

            var lastConsumptions = await _unitOfWork.Departments.Query
               .AsNoTracking()
               .Where(d => d.FactoryId == factoryId)
               .Select(d => d.ConsumptionLogs
               .Where(l => l.CapturedAt < date)
               .OrderByDescending(log => log.CapturedAt)
               .Select(log => log.ConsumptionValue)
               .FirstOrDefault()) // هيرجع IEnumerable<double> مثلاً
               .ToListAsync();

            var previousDayTotalFactoryConsumption = lastConsumptions.Sum();

            var currentConsumption = await _unitOfWork.Departments.Query
                .Where(d => d.FactoryId == factoryId && d.Id != department.Id)
                .Select(d => d.ConsumptionLogs.Where(l => l.CapturedAt >= date)
                    .OrderByDescending(log => log.CapturedAt)
                    .Select(log => log.ConsumptionValue)
                    .FirstOrDefault()) // بياخد آخر قيمة للقسم ده، لو مفيش هتبقى 0
                .ToListAsync();

            var currentTotalFactoryConsumption = currentConsumption.Sum();

            var newTotalFactoryConsumption = currentTotalFactoryConsumption + request.ConsumptionValue;

            var factoryConsumptionForToday = Math.Max(0, newTotalFactoryConsumption - previousDayTotalFactoryConsumption);

            var previousDayDepartmentConsumption = await _unitOfWork.ConsumptionLogs.Query
                .AsNoTracking()
                .Where(l => l.DepartmentId == department.Id && l.CapturedAt < date)
                .OrderByDescending(l => l.CapturedAt)
                .FirstOrDefaultAsync();


            var departmentConsumptionForToday = Math.Max(0, request.ConsumptionValue - (previousDayDepartmentConsumption == null ? 0 : previousDayDepartmentConsumption.ConsumptionValue));

            var finalDepartmentStatus = ConsumptionStatus.Normal;
            if (department.CurrentConsumptionLimit.HasValue)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(departmentConsumptionForToday, department.CurrentConsumptionLimit ?? 0);

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


            var log =  _mapper.Map<ConsumptionLog>(request);
            log.Status = finalDepartmentStatus;


            await _unitOfWork.ConsumptionLogs.AddAsync(log, cancellationToken);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result <= 0)
            {
                return Result<ConsumptionLogDto>.Failure("Error Adding The log in the data base");
            }

            if (finalDepartmentStatus != ConsumptionStatus.Normal || finalFactoryStatus != ConsumptionStatus.Normal)
            {
                await _mediator.Publish(new HighConsumptionDetectedEvent(departmentConsumptionForToday, finalFactoryStatus, factoryId, finalDepartmentStatus, department.Name, department.Id, log.Id));
            }

            var logDto = _mapper.Map<ConsumptionLogDto>(log);
            return Result<ConsumptionLogDto>.Success(logDto);
        }
    }
}
