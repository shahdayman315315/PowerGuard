using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Features.DepartmentManagerDashboard.Queries.GetDepartmentDailySummary;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.FactoryManagerDashboard.Queries.GetAllDepartmentsSummary
{
    public class GetAllDepartmentsSummaryQueryHandler :
        IRequestHandler<GetAllDepartmentsSummaryQuery, Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public GetAllDepartmentsSummaryQueryHandler(IUnitOfWork unitOfWork, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>> Handle(GetAllDepartmentsSummaryQuery request, CancellationToken cancellationToken)
        {
            var factory = await _unitOfWork.Factories.Query.Include(f => f.Departments).FirstOrDefaultAsync(f => f.Id == request.factoryId,cancellationToken);

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
                var query = new GetDepartmentDailySummaryQuery( department.Id);
                var result = await _mediator.Send(query, cancellationToken);

                if (result.IsSuccess)
                {
                    summaryDtos.Add(result.Data!);
                }
            }

            var totalFactoryConsumption = summaryDtos.Sum(s => s.TotalConsumption);
            if (totalFactoryConsumption > 0)
            {
                foreach (var departmentSummary in summaryDtos)
                {
                    departmentSummary.ShareOfFactoryConsumption = Math.Round((double)(departmentSummary.TotalConsumption / totalFactoryConsumption) * 100, 1);
                }
            }

            var sortedSummaries = summaryDtos
                  .OrderByDescending(s => s.ConsumptionPercentage)
                  .ThenBy(s => s.DepartmentName);

            return Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>.Success(sortedSummaries);
        }
    }
}
