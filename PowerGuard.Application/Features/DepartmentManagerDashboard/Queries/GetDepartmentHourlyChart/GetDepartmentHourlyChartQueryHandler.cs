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

namespace PowerGuard.Application.Features.DepartmentManagerDashboard.Queries.GetDepartmentHourlyChart
{
    public class GetDepartmentHourlyChartQueryHandler
        : IRequestHandler<GetDepartmentHourlyChartQuery, Result<IEnumerable<ChartPointDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDepartmentHourlyChartQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IEnumerable<ChartPointDto>>> Handle(GetDepartmentHourlyChartQuery request, CancellationToken cancellationToken)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(request.departmentId,cancellationToken);
            if (department is null)
            {
                return Result<IEnumerable<ChartPointDto>>.Failure("Department not found", 404);
            }

            var date = DateTime.UtcNow.Date;

            var latestLogBeforeToday = await _unitOfWork.ConsumptionLogs.Query
                .AsNoTracking().Where(l => l.DepartmentId == request.departmentId && l.CapturedAt < date)
                .OrderByDescending(l => l.CapturedAt)
                 .Select(l => new { l.ConsumptionValue, l.CapturedAt })
                .FirstOrDefaultAsync(cancellationToken);

            var logsToday = await _unitOfWork.ConsumptionLogs.Query
                .AsNoTracking().Where(l => l.DepartmentId == request.departmentId && l.CapturedAt >= date)
                .OrderBy(l => l.CapturedAt)
                .Select(l => new { l.ConsumptionValue, l.CapturedAt })
                .ToListAsync(cancellationToken);

            var previous = latestLogBeforeToday;

            var dtos = new List<ChartPointDto>();

            foreach (var l in logsToday)
            {
                var consumptionValue = Math.Max(0, l.ConsumptionValue - (previous == null ? 0 : previous.ConsumptionValue));
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
