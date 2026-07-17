using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Features.FactoryManagerDashboard.Queries.GetAllDepartmentsSummary;
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

namespace PowerGuard.Application.Features.FactoryManagerDashboard.Queries.GetFactoryDailySummary
{
    public class GetFactoryDailySummaryQueryHandler :
        IRequestHandler<GetFactoryDailySummaryQuery, Result<FactoryDailySummaryDto>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        private readonly IMediator _mediator;

        public GetFactoryDailySummaryQueryHandler(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork,
            IEnumerable<IConsumptionEvaluationStrategy> strategies, IMediator mediator)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _strategies = strategies;
            _mediator = mediator;
        }
        public async Task<Result<FactoryDailySummaryDto>> Handle(GetFactoryDailySummaryQuery request, CancellationToken cancellationToken)
        {
            var factory = await _unitOfWork.Factories.GetByIdAsync(request.factoryId, cancellationToken);

            if (factory is null)
            {
                return Result<FactoryDailySummaryDto>.Failure("Factory not found", 404);
            }

            var date = DateTime.UtcNow.Date;

            var latestConsumption = await _unitOfWork.Departments.Query
                .AsNoTracking().Where(d => d.FactoryId == request.factoryId)
                .Select(d => d.ConsumptionLogs
                .Where(l => l.CapturedAt >= date)
                .OrderByDescending(l => l.CapturedAt)
                .Select(l => l.ConsumptionValue)
                .FirstOrDefault())
                .SumAsync(cancellationToken);

            var latestConsumptionBeforeToday = await _unitOfWork.Departments.Query
                .AsNoTracking().Where(d => d.FactoryId == request.factoryId)
                .Select(d => d.ConsumptionLogs
                .Where(l => l.CapturedAt < date)
                .OrderByDescending(l => l.CapturedAt)
                .Select(l => l.ConsumptionValue)
                .FirstOrDefault())
                .SumAsync(cancellationToken);

            var latestConsumptionBeforeYesterday = await _unitOfWork.Departments.Query
               .AsNoTracking().Where(d => d.FactoryId == request.factoryId)
               .Select(d => d.ConsumptionLogs
               .Where(l => l.CapturedAt < date.AddDays(-1))
               .OrderByDescending(l => l.CapturedAt)
               .Select(l => l.ConsumptionValue)
               .FirstOrDefault())
               .SumAsync(cancellationToken);

            var factoryConsumptionForToday = Math.Max(0, latestConsumption - latestConsumptionBeforeToday);

            var currentLimit = factory.CurrentConsumptionLimit ?? 0;

            var percentage = currentLimit == 0 ? 0 : (factoryConsumptionForToday / currentLimit) * 100;

            var remainingAmount = currentLimit == 0 ? 0 : currentLimit - factoryConsumptionForToday;

            var factoryConsumptionForYesterday = Math.Max(0, latestConsumptionBeforeToday - latestConsumptionBeforeYesterday);

            double consumptionDiffAmount = 0;
            if (factoryConsumptionForYesterday > 0)
            {
                consumptionDiffAmount = (double)((factoryConsumptionForToday - factoryConsumptionForYesterday) / factoryConsumptionForYesterday) * 100;
            }

            var finalStatus = ConsumptionStatus.Normal;
            foreach (var strategy in _strategies)
            {
                var status = strategy.Evaluate(factoryConsumptionForToday, currentLimit);

                if (status > finalStatus)
                {
                    finalStatus = status;
                }
            }


            var summaryDto = new FactoryDailySummaryDto
            {
                FactoryName = factory.Name,
                TotalConsumption = Math.Round(factoryConsumptionForToday),
                ConsumptionPercentage = (double)Math.Round(percentage, 1),
                CurrentLimit = currentLimit,
                RemainingAmount = Math.Round(remainingAmount, 2),
                Status = finalStatus.ToString(),
                ComparisonWithYesterday = Math.Round(consumptionDiffAmount, 2),
            };

            var deptsSummaryQuery = new GetAllDepartmentsSummaryQuery(request.factoryId);

            var deptsSummaryResult = await _mediator.Send(deptsSummaryQuery, cancellationToken);

            if (deptsSummaryResult.IsSuccess && deptsSummaryResult.Data.Any())
            {
                var maxDept = deptsSummaryResult.Data.MaxBy(d => d.TotalConsumption);
                var minDept = deptsSummaryResult.Data.MinBy(d => d.TotalConsumption);

                if (maxDept != null)
                    summaryDto.TopConsumptionDept = new KeyValuePair<string, double>(maxDept.DepartmentName, maxDept.ConsumptionPercentage);

                if (minDept != null)
                    summaryDto.LeastConsumptionDept = new KeyValuePair<string, double>(minDept.DepartmentName, minDept.ConsumptionPercentage);

            }

            return Result<FactoryDailySummaryDto>.Success(summaryDto);
        }
    }
}
