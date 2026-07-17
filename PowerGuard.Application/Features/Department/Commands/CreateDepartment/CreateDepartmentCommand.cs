using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Commands.CreateDepartment
{
    public sealed record CreateDepartmentCommand(string Name, string? Description, string OperatingHours,
        decimal CurrentConsumptionLimit, string? ManagerId) : IRequest<Result<DepartmentDto>>;
    
}
