using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using PowerGuard.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Consumption.Queries.GetConsumptionLogs
{
    public class GetConsumptionLogsQueryHandler : IRequestHandler<GetConsumptionLogsQuery, Result<PagedResult<ConsumptionLogDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;

        public GetConsumptionLogsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<ConsumptionLogDto>>> Handle(GetConsumptionLogsQuery request, CancellationToken cancellationToken)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(request.departmentId, cancellationToken);

            if (department is null)
            {
                return Result<PagedResult<ConsumptionLogDto>>.Failure("Department not found", 404);
            }

            var logs = _unitOfWork.ConsumptionLogs.Query.AsNoTracking().Where(l => l.DepartmentId == request.departmentId)
                .OrderByDescending(l => l.CapturedAt);

            var pagedData = await logs.ProjectTo<ConsumptionLogDto>(_mapper.ConfigurationProvider)
                .ToPagedResultAsync(request.pageNumber, request.pageSize);

            return Result<PagedResult<ConsumptionLogDto>>.Success(pagedData);
        }
    }
}
