using MediatR;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Queries.GetAllDepartments
{
    public sealed record GetAllDepartmentsQuery(int? FactoryId = null):IRequest<Result<IEnumerable<DepartmentDto>>>;
    
}
