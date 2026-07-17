using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Queries.GetAvailableDepartmentManagers
{
    public class GetAvailableDepartmentManagersQueryHandler :
        IRequestHandler<GetAvailableDepartmentManagersQuery, Result<IEnumerable<ManagerDto>>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetAvailableDepartmentManagersQueryHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

        }
        public async Task<Result<IEnumerable<ManagerDto>>> Handle(GetAvailableDepartmentManagersQuery request, CancellationToken cancellationToken)
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("FactoryId")?.Value;

            if (string.IsNullOrEmpty(factoryIdClaim) || !int.TryParse(factoryIdClaim, out int factoryId))
            {
                return Result<IEnumerable<ManagerDto>>.Failure("Factory ID claim is missing or invalid.", 400);
            }


            var managers = await _userManager.GetUsersInRoleAsync("DepartmentManager");

            var result = managers
                .Where(u => u.FactoryId == factoryId)
                .Select(u => new ManagerDto
                {
                    Id = u.Id,
                    ManagerName = u.UserName!
                })
                .ToList();

            return Result<IEnumerable<ManagerDto>>.Success(result);
        }
    }
}
