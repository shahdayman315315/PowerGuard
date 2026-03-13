using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
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
        public readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        public DepartmentService(IUnitOfWork unitOfWork,UserManager<ApplicationUser> userManager 
            ,IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }
        public async Task<Result<IEnumerable<ManagerDto>>> GetAvailableManagersAsync(int factoryId)
        {
            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId);

            if (factory is null)
            {
                return Result<IEnumerable<ManagerDto>>.Failure("Factory not found.", 404);
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
            var result= await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
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
                return Result<bool>.Failure("User with this Email alredy exists");
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = _mapper.Map<ApplicationUser>(dto);
                user.FactoryId = factoryId;

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

            var departmentsDtos = await _unitOfWork.Departments.Query.Where(d=> d.FactoryId == factoryId).ProjectTo<DepartmentDto>(_mapper.ConfigurationProvider).ToListAsync();

           return Result<IEnumerable<DepartmentDto>>.Success(departmentsDtos);
        }
    }
}
