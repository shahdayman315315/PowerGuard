using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Commands.UpdateDepartment
{
    public sealed record UpdateDepartmentCommand(int departmentId, string Name, string? Description,string? OperatingHours):
        IRequest<Result<DepartmentDto>>;

}
