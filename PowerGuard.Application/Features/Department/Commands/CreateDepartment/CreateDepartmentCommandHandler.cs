using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PowerGuard.Application.Features.Department.Commands.CreateDepartment;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class CreateDepartmentCommandHandler :
        IRequestHandler<CreateDepartmentCommand, Result<DepartmentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public CreateDepartmentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _cache = cache;
        }
        public async Task<Result<DepartmentDto>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<DepartmentDto>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var factory = await _unitOfWork.Factories.Query.Include(f => f.Departments).FirstOrDefaultAsync(f => f.Id == factoryId);

            if (factory is null)
            {
                return Result<DepartmentDto>.Failure("Factory not found ", 404);
            }

            var existName = factory.Departments.Any(d => d.Name == request.Name);

            if (existName)
            {
                return Result<DepartmentDto>.Failure("Department Name already exists");
            }

            if (!string.IsNullOrEmpty(request.ManagerId))
            {
                var manager = await _userManager.FindByIdAsync(request.ManagerId);

                if (manager == null || manager.FactoryId != factoryId)
                {
                    return Result<DepartmentDto>.Failure("Manager not found or does not belong to this factory.");
                }
            }

            if (factory.CurrentConsumptionLimit < request.CurrentConsumptionLimit)
            {
                return Result<DepartmentDto>.Failure("Department consumption limit cannot exceed factory's current consumption limit.");
            }

            var department = _mapper.Map<Department>(request);
            department.FactoryId = factoryId;

            await _unitOfWork.Departments.AddAsync(department, cancellationToken);

            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);


            if (result > 0)
            {
                string cacheKey = $"Departments-Factory: {factoryId}";

                _cache.Remove(cacheKey);

                if (request.ManagerId is not null)
                {
                    var manager = await _userManager.FindByIdAsync(request.ManagerId);
                    manager.FactoryId = factory.Id;
                    manager.DepartmentId = department.Id;

                    var updateResult = await _userManager.UpdateAsync(manager);

                    if (!updateResult.Succeeded)
                    {
                        return Result<DepartmentDto>.Failure("Error happened while setting the manager");
                    }

                }

                var departmentDto = _mapper.Map<DepartmentDto>(department);
                return Result<DepartmentDto>.Success(departmentDto);
            }


            return Result<DepartmentDto>.Failure("Failed to save the department to the database.");
        }

       
    }
    
}
