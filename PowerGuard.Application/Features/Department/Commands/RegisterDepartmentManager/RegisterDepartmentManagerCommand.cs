using MediatR;
using PowerGuard.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Department.Commands.RegisterDepartmentManager
{
    public sealed record  RegisterDepartmentManagerCommand(string Email, int DepartmentId, string FullName,string Password) : 
        IRequest<Result<bool>>;
    
}
