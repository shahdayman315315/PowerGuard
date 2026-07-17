using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.FactoryManagerDashboard.Queries.GetFactoryHourlyChart
{
    public class GetFactoryHourlyChartQueryHandler
        : IRequestHandler<GetFactoryHourlyChartQuery, Result<IEnumerable<ChartPointDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetFactoryHourlyChartQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<IEnumerable<ChartPointDto>>> Handle(GetFactoryHourlyChartQuery request, CancellationToken cancellationToken)
        {
            var factory = await _unitOfWork.Factories.GetByIdAsync(request.factoryId, cancellationToken);

            if (factory is null)
                return Result<IEnumerable<ChartPointDto>>.Failure("Factory not found", 404);

            var date = DateTime.UtcNow.Date;

            var previousReadings = await _unitOfWork.Departments.Query
                .AsNoTracking()
                .Where(d => d.FactoryId == request.factoryId)
                .Select(d => new
                {
                    DeptId = d.Id,
                    LastValue = d.ConsumptionLogs
                        .Where(l => l.CapturedAt < date)
                        .OrderByDescending(l => l.CapturedAt)
                        .Select(l => l.ConsumptionValue)
                        .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.DeptId, x => x.LastValue,cancellationToken);

            var logsToday = await _unitOfWork.ConsumptionLogs.Query
                .AsNoTracking()
                .Where(l => l.Department.FactoryId == request.factoryId && l.CapturedAt >= date)
                .OrderBy(l => l.CapturedAt)
                .ToListAsync(cancellationToken);

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
                    // (بنثبت الوقت على أول الساعة عشان الرسمة تبقى منظمة (مثلاً 10:00)
                    CapturedAt = date.AddHours(g.Key),
                    ConsumptionValue = Math.Round(g.Sum(x => x.Delta), 2)
                })
                .OrderBy(x => x.CapturedAt)
                .ToList();

            return Result<IEnumerable<ChartPointDto>>.Success(hourlyData);
        }
    }
}
