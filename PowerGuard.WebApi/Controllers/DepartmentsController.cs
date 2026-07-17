using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Features.Department.Commands.CreateDepartment;
using PowerGuard.Application.Features.Department.Commands.DeleteDepartment;
using PowerGuard.Application.Features.Department.Commands.RegisterDepartmentManager;
using PowerGuard.Application.Features.Department.Commands.UpdateConsumptionLimit;
using PowerGuard.Application.Features.Department.Commands.UpdateDepartment;
using PowerGuard.Application.Features.Department.Queries.GetAllDepartments;
using PowerGuard.Application.Features.Department.Queries.GetAvailableDepartmentManagers;
using PowerGuard.Application.Features.Department.Queries.GetDepartment;
using PowerGuard.Application.Interfaces;
using PowerGuard.Application.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Admin,FactoryManager")]
    public class DepartmentsController : ControllerBase
    {
        private readonly ISender _sender;
        public DepartmentsController(ISender sender)
        {
            _sender = sender;
        }


        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateDepartmentCommand command)
        {
            var result = await _sender.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }


        [HttpPost("Add-DepartmentManager")]
        public async Task<IActionResult> AddDepartmentManager(RegisterDepartmentManagerCommand command)
        {
            var result= await _sender.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Message);
        }


        [HttpGet("AvailableDepartmentManagers")]
        public async Task<IActionResult> GetAvailableManagers()
        {
            var result = await _sender.Send(new GetAvailableDepartmentManagersQuery());

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result=await _sender.Send(new GetAllDepartmentsQuery());

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result=await _sender.Send(new GetDepartmentQuery(id));

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }


        [HttpPut("update")]
        public async Task<IActionResult> Update( UpdateDepartmentCommand command)
        {
            var result= await _sender.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }


        [HttpPatch("Update-ConsumptionLimit")]
        public async Task<IActionResult> UpdateConsumptionLimit(UpdateConsumptionLimitCommand command)
        {

            var result = await _sender.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result= await _sender.Send(new DeleteDepartmentCommand(id));

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }
    }
}
