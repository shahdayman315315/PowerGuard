using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Features.Consumption.Commands.DeleteConsumptionLog;
using PowerGuard.Application.Features.Consumption.Commands.EnterConsumption;
using PowerGuard.Application.Features.Consumption.Commands.UpdateConsumptionLog;
using PowerGuard.Application.Features.Consumption.Queries.GetConsumptionLogs;
using PowerGuard.Application.Interfaces;

namespace PowerGuard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="DepartmentManager,FactoryManager")]
    public class ConsumptionLogsController : ControllerBase
    {
        private readonly ISender _sender;
        public ConsumptionLogsController(ISender sender)
        {
            _sender = sender;
        }


        [HttpPost("enter-consumption")]
        public async Task<IActionResult> EnterConsumption(EnterConsumptionCommand command)
        {
            var result =await _sender.Send(command);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateConsumption(UpdateConsumptionLogCommand command)
        {
            var result = await _sender.Send(command);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsumption(int id)
        {
            var result = await _sender.Send(new DeleteConsumptionLogCommand(id));

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }

        [HttpGet("department-logs/{departmentId}")]
        public async Task<IActionResult> GetDepartmentLogs(int departmentId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _sender.Send(new GetConsumptionLogsQuery(departmentId, pageNumber, pageSize));

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);
        }

        

    }
}

