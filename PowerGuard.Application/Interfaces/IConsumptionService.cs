using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Interfaces
{
    public interface IConsumptionService
    {
        Task<Result<ConsumptionLogDto>> EnterConsumptionAsync(ConsumptionLogDto dto);
        Task<Result<PagedResult<ConsumptionLogDto>>> GetConsumptionLogsAsync(int departmentId,int pageNumber,int pageSize);
        Task<Result<DepartmentDailyConsumptionSummaryDto>> GetDepartmentDailySummaryAsync(int departmentId);
        Task<Result<FactoryDailySummaryDto>> GetFactoryDailySummaryAsync(int factoryId);
        Task<Result<IEnumerable<DepartmentDailyConsumptionSummaryDto>>> GetAllDepartmentsSummaryAsync(int factoryId);
        Task<Result<ConsumptionLogDto>> UpdateConsumptionLogAsync(UpdateConsumptionLogDto dto);
        Task<Result<bool>> DeleteConsumptionLogAsync(int logId);

    }
}
