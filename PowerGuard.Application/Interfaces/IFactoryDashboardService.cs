using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Interfaces
{
    public interface IFactoryDashboardService
    {
        Task<Result<FactoryDailySummaryDto>> GetFactoryDailySummaryAsync(int factoryId);
        Task<Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>> GetAllDepartmentsSummaryAsync(int factoryId);
        Task<Result<IEnumerable<ChartPointDto>>> GetFactoryHourlyChartAsync(int factoryId);

    }
}
