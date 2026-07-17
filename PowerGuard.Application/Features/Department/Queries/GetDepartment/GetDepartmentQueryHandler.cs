using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Queries.GetDepartment
{
    public class GetDepartmentQueryHandler : IRequestHandler<GetDepartmentQuery, Result<DepartmentDto>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetDepartmentQueryHandler(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork,IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Result<DepartmentDto>> Handle(GetDepartmentQuery request, CancellationToken cancellationToken)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<DepartmentDto>.Failure("Factory ID claim is missing or invalid.", 400);
            }

            var factory = await _unitOfWork.Factories.GetByIdAsync(factoryId,cancellationToken);

            if (factory is null)
            {
                return Result<DepartmentDto>.Failure("Factory not found ", 404);
            }

            var departmentDto = await _unitOfWork.Departments.Query.Where(d => d.Id == request.DepartmentId && d.FactoryId == factoryId).ProjectTo<DepartmentDto>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(cancellationToken);

            if (departmentDto is null)
            {
                return Result<DepartmentDto>.Failure("Department not found");
            }


            return Result<DepartmentDto>.Success(departmentDto);
        }
    }
}
