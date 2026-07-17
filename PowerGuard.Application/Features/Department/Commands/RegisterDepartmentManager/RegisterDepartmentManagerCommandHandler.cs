using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Commands.RegisterDepartmentManager
{
    public class RegisterDepartmentManagerCommandHandler :
        IRequestHandler<RegisterDepartmentManagerCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public RegisterDepartmentManagerCommandHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }
        public async Task<Result<bool>> Handle(RegisterDepartmentManagerCommand request, CancellationToken cancellationToken)
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

            var department = factory.Departments.FirstOrDefault(d => d.Id == request.DepartmentId);

            if (department is null)
            {
                return Result<bool>.Failure("Department not found in this factory.");
            }

            var existUser = await _userManager.FindByEmailAsync(request.Email);

            if (existUser is not null)
            {
                return Result<bool>.Failure("User with this Email already exists");
            }

            var strategy = await _unitOfWork.CreateExecutionStrategy(cancellationToken);

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var user = _mapper.Map<ApplicationUser>(request);
                    user.FactoryId = factoryId;
                    user.DepartmentId = department.Id;

                    var result = await _userManager.CreateAsync(user, request.Password);

                    if (!result.Succeeded)
                    {
                        string errors = string.Join(',', result.Errors.Select(e => e.Description));

                        return Result<bool>.Failure($"Error creating the manager: {errors} ");
                    }

                    await _userManager.AddToRoleAsync(user, "DepartmentManager");

                    department.ManagerId = user.Id;
                    _unitOfWork.Departments.Update(department);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

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
    }
}
