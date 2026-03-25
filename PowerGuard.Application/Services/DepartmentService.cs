using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Events;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IEnumerable<IConsumptionEvaluationStrategy> _strategies;
        private readonly IMediator _mediator;

        public DepartmentService(IUnitOfWork unitOfWork,UserManager<ApplicationUser> userManager 
            ,IHttpContextAccessor httpContextAccessor, IMapper mapper,IMemoryCache cache, 
            IEnumerable<IConsumptionEvaluationStrategy> strategies, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _cache = cache;
            _strategies = strategies;
            _mediator = mediator;
        }
        public async Task<Result<IEnumerable<ManagerDto>>> GetAvailableManagersAsync()
        {

            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result< IEnumerable<ManagerDto>>.Failure("Factory ID claim is missing or invalid.", 400);
            }


            var managers = await _userManager.GetUsersInRoleAsync("DepartmentManager");

            var result = managers
                .Where(u => u.FactoryId == factoryId) 
                .Select(u => new ManagerDto
                {
                    Id = u.Id,
                    ManagerName = u.UserName 
                })
                .ToList();

            return Result<IEnumerable<ManagerDto>>.Success(result);
        }

        public async Task<Result<CreateDepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto dto)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if(string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<CreateDepartmentDto>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var factory = await _unitOfWork.Factories.Query.Include(f => f.Departments).FirstOrDefaultAsync(f => f.Id == factoryId);

            if (factory is null)
            {
                return Result<CreateDepartmentDto>.Failure("Factory not found ", 404);
            }

            var existName= factory.Departments.Any(d=>d.Name==dto.Name);

            if (existName)
            {
                return Result<CreateDepartmentDto>.Failure("Department Name already exists");
            }

            if (!string.IsNullOrEmpty(dto.ManagerId))
            {
                var manager = await _userManager.FindByIdAsync(dto.ManagerId);

                if (manager == null || manager.FactoryId != factoryId)
                {
                    return Result<CreateDepartmentDto>.Failure("Manager not found or does not belong to this factory.");
                }
            }

            if (factory.CurrentConsumptionLimit < dto.CurrentConsumptionLimit)
            {
                return Result<CreateDepartmentDto>.Failure("Department consumption limit cannot exceed factory's current consumption limit.");
            }

            var department=_mapper.Map<Department>(dto);
            department.FactoryId =factoryId;

            await _unitOfWork.Departments.AddAsync(department);

            var result = await _unitOfWork.SaveChangesAsync();


            if (result > 0)
            {
                string cacheKey = $"Departments-Factory: {factoryId}";

                _cache.Remove(cacheKey);

                if (dto.ManagerId is not null)
                {
                    var manager = await _userManager.FindByIdAsync(dto.ManagerId);
                    manager.FactoryId = factory.Id;
                    manager.DepartmentId = department.Id;

                    var updateResult = await _userManager.UpdateAsync(manager);

                    if (!updateResult.Succeeded)
                    {
                        return Result<CreateDepartmentDto>.Failure("Error happened while setting the manager");
                    }

                }

                return Result<CreateDepartmentDto>.Success(dto);
            }


            return Result<CreateDepartmentDto>.Failure("Failed to save the department to the database.");


        }

        public async Task<Result<bool>> RegisterDepartmentManager(RegisterManagerDto dto)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<bool>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var factory = await _unitOfWork.Factories.Query.Include(f => f.Departments).FirstOrDefaultAsync(f => f.Id == factoryId);

            if (factory is null)
            {
                return Result<bool>.Failure("Factory not found ", 404);
            }

            var department = factory.Departments.FirstOrDefault(d => d.Id == dto.DepartmentId);

            if (department is null)
            {
                return Result<bool>.Failure("Department not found in this factory.");
            }

            var existUser=await _userManager.FindByEmailAsync(dto.Email);

            if (existUser is not null)
            {
                return Result<bool>.Failure("User with this Email already exists");
            }

            var strategy = await _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var user = _mapper.Map<ApplicationUser>(dto);
                    user.FactoryId = factoryId;
                    user.DepartmentId=department.Id;

                    var result = await _userManager.CreateAsync(user, dto.Password);

                    if (!result.Succeeded)
                    {
                        string errors = string.Join(',', result.Errors.Select(e => e.Description));

                        return Result<bool>.Failure($"Error creating the manager: {errors} ");
                    }

                    await _userManager.AddToRoleAsync(user, "DepartmentManager");

                    department.ManagerId = user.Id;
                    _unitOfWork.Departments.Update(department);

                    await _unitOfWork.SaveChangesAsync();

                    transaction.Commit();

                    return Result<bool>.Success(true, "Department Manager assigned Successfully");


                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    return Result<bool>.Failure(ex.Message);
                }
            });
           
           
        }

        public async Task<Result<DepartmentDto>> GetByIdAsync(int id)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<DepartmentDto>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId);

            if (factory is null)
            {
                return Result<DepartmentDto>.Failure("Factory not found ", 404);
            }

            var departmentDto=await _unitOfWork.Departments.Query.Where(d => d.Id == id && d.FactoryId == factoryId).ProjectTo<DepartmentDto>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
           
            if (departmentDto is null)
            {
                return Result<DepartmentDto>.Failure("Department not found");
            }


            return Result<DepartmentDto>.Success(departmentDto);
        }

        public async Task<Result<IEnumerable<DepartmentDto>>> GetAllAsync(int? factoryId = null)
        {
            if (!factoryId.HasValue)
            {
                var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

                if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryIdfromCliam))
                {
                    return Result<IEnumerable<DepartmentDto>>.Failure("Factory ID claim is missing or invalid.", 400);
                }

                factoryId = factoryIdfromCliam;
            }

            string cacheKey= $"Departments-Factory: {factoryId}";

            if(!_cache.TryGetValue(cacheKey, out IEnumerable<DepartmentDto> departmentsDtos))
            {
                departmentsDtos = await _unitOfWork.Departments.Query.Where(d => d.FactoryId == factoryId).ProjectTo<DepartmentDto>(_mapper.ConfigurationProvider).ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromMinutes(30))
               .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(cacheKey, departmentsDtos, cacheOptions);
            }

            return Result<IEnumerable<DepartmentDto>>.Success(departmentsDtos);
        }

        public async Task<Result<DepartmentDto>> UpdateAsync(UpdateDepartmentDto dto,int departmentId)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<DepartmentDto>.Failure("Factory ID claim is missing or invalid.", 400);
            }
            var department=await _unitOfWork.Departments.GetByIdAsync(departmentId);

            if(department is null)
            {
                return Result<DepartmentDto>.Failure("Department not found", 404);
            }

            if(department.Name != dto.Name && _unitOfWork.Departments.Query.Any(d=>d.Name==dto.Name && d.FactoryId==factoryId))
            {
                return Result<DepartmentDto>.Failure("Department with the Name alredy exists.");
            }

            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId);
           

            _mapper.Map(dto, department);
            _unitOfWork.Departments.Update(department);
            var result=await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                string cacheKey = $"Departments-Factory: {department.FactoryId}";

                _cache.Remove(cacheKey);

                var departmentDto=_mapper.Map<DepartmentDto>(department);

                return Result<DepartmentDto>.Success(departmentDto);

            }

            return Result<DepartmentDto>.Failure("Can't update department in the database");
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<bool>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var department = await _unitOfWork.Departments.Query.FirstOrDefaultAsync(d => d.Id == id && d.FactoryId == factoryId);

            if(department is null)
            {
                return Result<bool>.Failure("Department not found", 404);
            }

            _unitOfWork.Departments.Delete(department);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                string cacheKey = $"Departments-Factory: {department.FactoryId}";

                _cache.Remove(cacheKey);

                return Result<bool>.Success(true, "Department deleted successfully");
            }

            return Result<bool>.Failure("Can't delete department from the data base");
        }

        public async Task<Result<bool>> UpdateConsumptionLimit(UpdateConsumptionLimitDto dto, int departmentId, string userId)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<bool>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var factory=await _unitOfWork.Factories.GetByIdAsync(factoryId);

            if(factory is null)
            {
                return Result<bool>.Failure("Departmentnot found");

            }

            if (factory.CurrentConsumptionLimit < dto.NewLimit)
            {
                return Result<bool>.Failure("Department Limit can't be greater tha factory limit");
            }

            var department = await _unitOfWork.Departments.Query.FirstOrDefaultAsync(d => d.Id == departmentId && d.FactoryId == factoryId);

            if (department is null)
            {
                return Result<bool>.Failure("Department not found", 404);
            }

            var lastReading = await _unitOfWork.ConsumptionLogs.Query.Where(d => d.DepartmentId == departmentId).OrderByDescending(l => l.CapturedAt).FirstOrDefaultAsync();

            var departmentStatus = ConsumptionStatus.Normal;
            
            if(lastReading != null)
            {
                foreach (var strategy in _strategies)
                {
                    var status = strategy.Evaluate(lastReading.ConsumptionValue, dto.NewLimit);

                    if (status > departmentStatus)
                    {
                        departmentStatus = status;
                    }
                }

            }
            

            department.CurrentConsumptionLimit = dto.NewLimit;

            var limitHistory = new LimitHistory
            {
                FactoryId = factoryId,
                DepartmentId = departmentId,
                LimitValue = dto.NewLimit,
                CreatedAt = DateTime.UtcNow,
                ActiveFrom = DateTime.UtcNow,
                SetBy = userId
            };

            await _unitOfWork.LimitHistories.AddAsync(limitHistory);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result <= 0)
            {
                return Result<bool>.Failure("Failed to update the consumption limit in the database.");

            }

            if (lastReading is not null && departmentStatus != ConsumptionStatus.Normal)
            {
                await _mediator.Publish(new HighConsumptionDetectedEvent(lastReading.ConsumptionValue, ConsumptionStatus.Normal, factoryId, departmentStatus, department.Name, departmentId, lastReading.Id));

            }

            return Result<bool>.Success(true);

        }
    }
}
