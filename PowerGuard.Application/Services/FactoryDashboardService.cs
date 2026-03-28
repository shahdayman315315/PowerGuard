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
    public class FactoryDashboardService:IFactoryDashboardService
    {
        private readonly IDepartmentDashboardService _departmentDashboardService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        public FactoryDashboardService(IDepartmentDashboardService departmentDashboardService, IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork , IEnumerable<IConsumptionEvaluationStrategy> strategies)
        {
            _departmentDashboardService = departmentDashboardService;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _strategies = strategies;

        }
        public async Task<Result<FactoryDailySummaryDto>> GetFactoryDailySummaryAsync(int factoryId)
        {
            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId);

            if (factory is null)
            {
                return Result<FactoryDailySummaryDto>.Failure("Factory not found", 404);
            }

            var date = DateTime.UtcNow.Date;

            var latestConsumption = await _unitOfWork.Departments.Query
                .AsNoTracking().Where(d => d.FactoryId == factoryId)
                .Select(d => d.ConsumptionLogs
                .Where(l => l.CapturedAt >= date)
                .OrderByDescending(l => l.CapturedAt)
                .Select(l => l.ConsumptionValue)
                .FirstOrDefault())
                .SumAsync();

            var latestConsumptionBeforeToday = await _unitOfWork.Departments.Query
                .AsNoTracking().Where(d => d.FactoryId == factoryId)
                .Select(d => d.ConsumptionLogs
                .Where(l => l.CapturedAt < date)
                .OrderByDescending(l => l.CapturedAt)
                .Select(l => l.ConsumptionValue)
                .FirstOrDefault())
                .SumAsync();

            var latestConsumptionBeforeYesterday = await _unitOfWork.Departments.Query
               .AsNoTracking().Where(d => d.FactoryId == factoryId)
               .Select(d => d.ConsumptionLogs
               .Where(l => l.CapturedAt < date.AddDays(-1))
               .OrderByDescending(l => l.CapturedAt)
               .Select(l => l.ConsumptionValue)
               .FirstOrDefault())
               .SumAsync();

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

            var deptsSummaryResult = await GetAllDepartmentsSummaryAsync(factoryId);

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


        public async Task<Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>> GetAllDepartmentsSummaryAsync(int factoryId)
        {
            var factory = await _unitOfWork.Factories.Query.Include(f => f.Departments).FirstOrDefaultAsync(f => f.Id == factoryId);

            if (factory is null)
            {
                return Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>.Failure("Factory not found", 404);
            }

            List<DepartmentDailyConsumptionSummaryDto> summaryDtos = new List<DepartmentDailyConsumptionSummaryDto>();

            var departments = factory.Departments;

            if (!departments.Any())
            {
                return Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>.Failure("No departments found for this factory", 404);
            }

            foreach (var department in departments)
            {
                var result = await _departmentDashboardService.GetDepartmentDailySummaryAsync(department.Id);

                if (result.IsSuccess)
                {
                    summaryDtos.Add(result.Data!);
                }
            }

            var totalFactoryConsumption = summaryDtos.Sum(s => s.TotalConsumption);
            if(totalFactoryConsumption > 0)
            {
                foreach (var departmentSummary in summaryDtos)
                {
                    departmentSummary.ShareOfFactoryConsumption=Math.Round((double)(departmentSummary.TotalConsumption/totalFactoryConsumption)*100,1);
                }
            }

            var sortedSummaries = summaryDtos
                  .OrderByDescending(s => s.ConsumptionPercentage)
                  .ThenBy(s => s.DepartmentName);

            return Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>.Success(sortedSummaries);
        }

        public async Task<Result<IEnumerable<ChartPointDto>>> GetFactoryHourlyChartAsync(int factoryId)
        {
            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId);

            if (factory is null)
                return Result<IEnumerable<ChartPointDto>>.Failure("Factory not found", 404);

            var date = DateTime.UtcNow.Date;

            var previousReadings = await _unitOfWork.Departments.Query
                .AsNoTracking()
                .Where(d => d.FactoryId == factoryId)
                .Select(d => new
                {
                    DeptId = d.Id,
                    LastValue = d.ConsumptionLogs
                        .Where(l => l.CapturedAt < date)
                        .OrderByDescending(l => l.CapturedAt)
                        .Select(l => l.ConsumptionValue)
                        .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.DeptId, x => x.LastValue);

            var logsToday = await _unitOfWork.ConsumptionLogs.Query
                .AsNoTracking()
                .Where(l => l.Department.FactoryId == factoryId && l.CapturedAt >= date)
                .OrderBy(l => l.CapturedAt)
                .ToListAsync();

            var dailyChanges = new List<(DateTime Time, decimal Delta)>();

            foreach (var log in logsToday)
            {
                previousReadings.TryGetValue(log.DepartmentId, out decimal prevValue);

                var delta = Math.Max(0, log.ConsumptionValue - prevValue);

                dailyChanges.Add((log.CapturedAt, delta));

                previousReadings[log.DepartmentId] = log.ConsumptionValue;
            }

            // 5. التجميع النهائي بالساعة (Grouping by Hour)
            var hourlyData = dailyChanges
                .GroupBy(x => x.Time.Hour)
                .Select(g => new ChartPointDto
                {
                    // بنثبت الوقت على أول الساعة عشان الرسمة تبقى منظمة (مثلاً 10:00)
                    CapturedAt = date.AddHours(g.Key),
                    ConsumptionValue = Math.Round(g.Sum(x => x.Delta), 2)
                })
                .OrderBy(x => x.CapturedAt)
                .ToList();

            return Result<IEnumerable<ChartPointDto>>.Success(hourlyData);
        }
    }
}
