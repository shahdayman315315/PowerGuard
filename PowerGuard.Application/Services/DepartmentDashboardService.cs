using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Services
{
    public class DepartmentDashboardService : IDepartmentDashboardService
    {
        private readonly IConsumptionService _consumptionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        public DepartmentDashboardService(IConsumptionService consumptionService, IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork, IEnumerable<IConsumptionEvaluationStrategy> strategies)
        {
            _consumptionService = consumptionService;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _strategies = strategies;

        }

        public async Task<Result<DepartmentDailyConsumptionSummaryDto>> GetDepartmentDailySummaryAsync(int departmentId)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (department is null)
            {
                return Result<DepartmentDailyConsumptionSummaryDto>.Failure("Department not found", 404);
            }

            var date = DateTime.UtcNow.Date;

            var latestLog = await _unitOfWork.ConsumptionLogs.Query.AsNoTracking()
                .Where(l => l.DepartmentId == departmentId && l.CapturedAt >= date).OrderByDescending(l => l.CapturedAt)
                .FirstOrDefaultAsync();

            var latestLogBeforeToday = await _unitOfWork.ConsumptionLogs.Query.AsNoTracking()
                .Where(l => l.DepartmentId == departmentId && l.CapturedAt < date).OrderByDescending(l => l.CapturedAt)
                .FirstOrDefaultAsync();

            var latestLogBeforeYesterday = await _unitOfWork.ConsumptionLogs.Query.AsNoTracking()
                .Where(l => l.DepartmentId == departmentId && l.CapturedAt < date.AddDays(-1)).OrderByDescending(l => l.CapturedAt)
                .FirstOrDefaultAsync();

            var actualConsumptionForToday = Math.Max(0, (latestLog == null ? 0 : latestLog.ConsumptionValue) - (latestLogBeforeToday == null ? 0 : latestLogBeforeToday.ConsumptionValue));

            var currentLimit = department.CurrentConsumptionLimit;

            var percentage = currentLimit == null ? 0 : (double)(actualConsumptionForToday / currentLimit) * 100;

            var remainingAmount = currentLimit - actualConsumptionForToday;

            var totalConsumptionYesterday = Math.Max(0, ((latestLogBeforeToday == null ? 0 : latestLogBeforeToday.ConsumptionValue) - (latestLogBeforeYesterday == null ? 0 : latestLogBeforeYesterday.ConsumptionValue)));

            double consumptionDiffAmount = 0;
            if (totalConsumptionYesterday > 0)
            {
                consumptionDiffAmount = (double)((actualConsumptionForToday - totalConsumptionYesterday) / totalConsumptionYesterday) * 100;
            }

            var finalStatus = ConsumptionStatus.Normal;
            foreach (var strategy in _strategies)
            {
                var status = strategy.Evaluate(actualConsumptionForToday, currentLimit ?? 0);

                if (status > finalStatus)
                {
                    finalStatus = status;
                }
            }

            var summaryDto = new DepartmentDailyConsumptionSummaryDto
            {
                DepartmentName = department.Name,
                TotalConsumption = Math.Round(actualConsumptionForToday, 2),
                CurrentLimit = currentLimit ?? 0,
                ConsumptionPercentage = Math.Round(percentage, 1),
                RemainingAmount = Math.Round(remainingAmount ?? 0, 2),
                Status = finalStatus.ToString(),
                LastReadingValue = latestLog == null ? 0 : latestLog.ConsumptionValue,
                LastReadingAt = latestLog == null ? null : latestLog.CapturedAt,
                ComparisonWithYesterday = (double)Math.Round(consumptionDiffAmount, 2)
            };

            return Result<DepartmentDailyConsumptionSummaryDto>.Success(summaryDto);
        }

        public async Task<Result<IEnumerable<ChartPointDto>>> GetDepartmentHourlyChartAsync(int departmentId)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (department is null)
            {
                return Result<IEnumerable<ChartPointDto>>.Failure("Department not found", 404);
            }

            var date = DateTime.UtcNow.Date;
 
            var latestLogBeforeToday=await _unitOfWork.ConsumptionLogs.Query
                .AsNoTracking().Where(l=>l.DepartmentId == departmentId && l.CapturedAt<date)
                .OrderByDescending(l=>l.CapturedAt)
                 .Select(l => new {  l.ConsumptionValue,  l.CapturedAt })
                .FirstOrDefaultAsync();

            var logsToday = await _unitOfWork.ConsumptionLogs.Query
                .AsNoTracking().Where(l => l.DepartmentId == departmentId && l.CapturedAt >= date)
                .OrderBy(l => l.CapturedAt)
                .Select(l=> new {  l.ConsumptionValue , l.CapturedAt})
                .ToListAsync();

            var previous=latestLogBeforeToday;

            var dtos=new List<ChartPointDto>();

            foreach (var l in logsToday)
            {
                var consumptionValue =Math.Max(0,l.ConsumptionValue - (previous== null? 0 :previous.ConsumptionValue));
                var time = l.CapturedAt;

                previous = l;
                dtos.Add(new ChartPointDto
                {
                    CapturedAt = time,
                    ConsumptionValue = consumptionValue
                });
            }


            return Result<IEnumerable<ChartPointDto>>.Success(dtos);
        }
    }
}
