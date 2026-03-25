using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<Result<IEnumerable<ManagerDto>>> GetAvailableManagersAsync();
        Task<Result<CreateDepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto dto);
        Task<Result<bool>> RegisterDepartmentManager(RegisterManagerDto dto);
        Task<Result<DepartmentDto>> GetByIdAsync(int id);
        Task<Result<IEnumerable<DepartmentDto>>> GetAllAsync(int? factoryId=null);
        Task<Result<DepartmentDto>> UpdateAsync(UpdateDepartmentDto dto,int departmentId);
        Task<Result<bool>> UpdateConsumptionLimit(UpdateConsumptionLimitDto dto,int departmentId,string userId);
        Task<Result<bool>> DeleteAsync(int id);

    }
}
