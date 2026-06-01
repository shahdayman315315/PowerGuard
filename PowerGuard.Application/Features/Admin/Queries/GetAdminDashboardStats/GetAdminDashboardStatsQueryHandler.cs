using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Admin.Queries.GetAdminDashboardStats
{
    public class GetAdminDashboardStatsQueryHandler : IRequestHandler<GetAdminDashboardStatsQuery, Result<AdminDashboardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAdminDashboardStatsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
        {

            var statsQuery = await _unitOfWork.Factories.Query
                             .GroupBy(f => f.Status)
                             .Select(group => new
                             {
                              Status = group.Key,
                              Count = group.Count()
                              })
                             .ToListAsync(cancellationToken);

            var dto = new AdminDashboardDto
            {
                TotalFactories = statsQuery.Sum(x => x.Count),
                PendingFactories = statsQuery.FirstOrDefault(x => x.Status == FactoryStatus.Pending)?.Count ?? 0,
                ActiveFactories = statsQuery.FirstOrDefault(x => x.Status == FactoryStatus.Approved)?.Count ?? 0
            };

            return Result<AdminDashboardDto>.Success(dto);
        }
    }
}
