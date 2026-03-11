using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Interfaces
{
    public interface IAdminService
    {
        Task<Result<AdminDashboardDto>> GetAdminDashboardStats();
        Task<Result<bool>> ReviewFactory(ReviewFactoryDto dto);
    }
}
