using Microsoft.EntityFrameworkCore.Migrations.Operations;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Interfaces
{
    public interface IFactoryService
    {
        Task<Result<CreateFactoryDto>> CreateFactory(CreateFactoryDto dto,string userId);
        Task<Result<List<FactoryDto>>> GetAllFactories();
        Task<Result<FactoryDto>> GetFactoryById(int id);
        Task<Result<FactoryDto>> UpdateFactory(int id, UpdateFactoryDto dto);
        Task<Result<bool>> DeleteFactory(int id);
        Task<Result<List<FactoryDetailsDto>>> GetPendingFactories();
        Task<Result<List<FactoryDto>>> GetActiveFactories();
        Task<Result<bool>> UpdateConsumptionLimit(int factoryId, UpdateConsumptionLimitDto dto, string userId);
    }
}
