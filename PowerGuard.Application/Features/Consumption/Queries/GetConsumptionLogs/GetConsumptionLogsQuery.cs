using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Consumption.Queries.GetConsumptionLogs
{
    public sealed record GetConsumptionLogsQuery(int departmentId, int pageNumber, int pageSize) :IRequest<Result<PagedResult<ConsumptionLogDto>>>;
    
}
