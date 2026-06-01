using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Commands.DeleteDepartment
{
    public class DeleteDepartmentCommandHandler :
        IRequestHandler<DeleteDepartmentCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;

        public DeleteDepartmentCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
        }
        public async Task<Result<bool>> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<bool>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var department = await _unitOfWork.Departments.Query.FirstOrDefaultAsync(d => d.Id == request.DepartmentId && d.FactoryId == factoryId);

            if (department is null)
            {
                return Result<bool>.Failure("Department not found", 404);
            }

            _unitOfWork.Departments.Delete(department);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                string cacheKey = $"Departments-Factory: {department.FactoryId}";

                _memoryCache.Remove(cacheKey);

                return Result<bool>.Success(true, "Department deleted successfully");
            }

            return Result<bool>.Failure("Can't delete department from the data base");
        }
    }
}
