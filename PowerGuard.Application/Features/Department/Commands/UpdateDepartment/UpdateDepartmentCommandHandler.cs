using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Commands.UpdateDepartment
{
    public class UpdateDepartmentCommandHandler :
        IRequestHandler<UpdateDepartmentCommand, Result<DepartmentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateDepartmentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Result<DepartmentDto>> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<DepartmentDto>.Failure("Factory ID claim is missing or invalid.", 400);
            }
            var department = await _unitOfWork.Departments.GetByIdAsync(request.departmentId);

            if (department is null)
            {
                return Result<DepartmentDto>.Failure("Department not found", 404);
            }

            if (department.Name != request.Name && _unitOfWork.Departments.Query.Any(d => d.Name == request.Name && d.FactoryId == factoryId))
            {
                return Result<DepartmentDto>.Failure("Department with the Name alredy exists.");
            }

            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId);


            _mapper.Map(request, department);
            _unitOfWork.Departments.Update(department);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                string cacheKey = $"Departments-Factory: {department.FactoryId}";

                await _cache.RemoveAsync(cacheKey);

                var departmentDto = _mapper.Map<DepartmentDto>(department);

                return Result<DepartmentDto>.Success(departmentDto);

            }

            return Result<DepartmentDto>.Failure("Can't update department in the database");
        }
    }
}
